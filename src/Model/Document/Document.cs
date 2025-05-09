using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
using NMLServer.Model.Statement;

namespace NMLServer.Model;

using DefinitionsMap = Dictionary<IdentifierToken, List<IdentifierToken>>;

internal partial class Document
{
    private string _source;
    public int version { get; private set; }
    public readonly DocumentUri Uri;

    private List<Token> _tokens = [];
    private List<StatementAST>? _statements = [];
    private readonly List<int> _lineLengths = [];
    private readonly DefinitionsMap _definedSymbols;
    private readonly IdentifierComparer _symbolComparer = new();
    private readonly List<Token> _unexpectedTokens = [];

    private string source
    {
        get => _source;
        set => _symbolComparer.context = _source = value;
    }

    private PositionConverter MakeConverter() => new(_lineLengths);

    public Document(TextDocumentItem item)
    {
        version = item.Version;
        _source = item.Text;
        Uri = item.Uri;
        _symbolComparer.context = _source;
        _definedSymbols = new DefinitionsMap(_symbolComparer);
        MakeTokensFromScratch();
        MakeStatementsFromScratch();
        MakeDefinitionsFromScratch();
    }

    // TODO incremental
    private void ProvideSemanticTokens(in SemanticTokensBuilder builder, int indLeft, int indRight)
    {
        var converter = MakeConverter();
        for (int i = indLeft; i < indRight; ++i)
        {
            var token = _tokens[i];
            if (token is CommentToken comment)
            {
                foreach (var (offset, length) in converter.LocalToProtocol(comment))
                    builder.Push(offset, length, comment.semanticType);
                continue;
            }
            var type = token switch
            {
                IdentifierToken id when _definedSymbols.ContainsKey(id) => SemanticTokenTypes.Function,
                { semanticType: { } semanticType } => semanticType,
                _ => null
            };
            if (type is not null)
                builder.Push(converter.LocalToProtocol(token.start), token.length, type);
        }
    }

    public void ProvideSemanticTokens(in SemanticTokensBuilder builder, Range range)
    {
        FindTokensAtRangeBounds(ProtocolToLocal(range), out var left, out var right);
        ProvideSemanticTokens(in builder, int.Max(left.index, 0), right.index);
    }

    public void ProvideSemanticTokens(in SemanticTokensBuilder builder)
        => ProvideSemanticTokens(in builder, 0, _tokens.Count);

    public List<Location>? TryGetDefinitionLocations(DocumentUri uri, Position position)
    {
        if (GetToken(position) is not IdentifierToken symbol
            || !_definedSymbols.TryGetValue(symbol, out var definitions)
           )
            return null;

        List<Location> locations = new(definitions.Count);
        var length = symbol.length;
        var converter = MakeConverter();
        foreach (var definition in definitions)
        {
            var definitionStart = definition.start;
            var start = converter.LocalToProtocol(definitionStart);
            var end = start with { Character = start.Character + length };
            locations.Add(new Location(uri, new Range(start, end)));
        }
        return locations;
    }

    private Token? GetToken(Position pos) => GetToken(ProtocolToLocal(pos));

    private Token? GetToken(int offset)
    {
        for (int left = 0, right = _tokens.Count - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var current = _tokens[mid];
            if (offset < current.start)
            {
                right = mid - 1;
                continue;
            }
            if (offset <= current.end)
            {
                return current;
            }
            left = mid + 1;
        }
        return null;
    }

    private int ProtocolToLocal(Position position)
    {
        var start = position.Character;
        for (int i = 0; i < position.Line; ++i)
            start += _lineLengths[i];
        return start;
    }

    private void MakeTokensFromScratch()
    {
        _tokens.Clear();
        _lineLengths.Clear();
        Lexer lexer = new(source, _lineLengths);
        lexer.ProcessUntilFileEnd(in _tokens);
    }

    private void MakeStatementsFromScratch()
    {
        _unexpectedTokens.Clear();
        ParsingState state = new(_tokens, in _unexpectedTokens);
        StatementASTBuilder builder = new();
        while (builder.Make(ref state))
        { }
        _statements = builder.root;
    }

    private void MakeDefinitionsFromScratch()
    {
        _definedSymbols.Clear();
        if (_statements is null)
        {
            return;
        }
        Stack<InnerStatementNode> parents = [];
        foreach (var child in _statements)
        {
            if (child is InnerStatementNode parent)
                parents.Push(parent);

            if (child is not ISymbolSource { symbol: { } symbol })
                continue;

            if (_definedSymbols.TryGetValue(symbol, out var equalSymbols))
            {
                equalSymbols.Add(symbol);
                continue;
            }
            _definedSymbols[symbol] = [symbol];
        }
        while (parents.TryPop(out var node))
        {
            foreach (var child in node.Children ?? [])
            {
                if (child is InnerStatementNode parent)
                    parents.Push(parent);

                if (child is not ISymbolSource { symbol: { } symbol })
                {
                    continue;
                }
                if (_definedSymbols.TryGetValue(symbol, out var equalSymbols))
                {
                    equalSymbols.Add(symbol);
                    continue;
                }
                _definedSymbols[symbol] = [symbol];
            }
        }
    }

    public List<Diagnostic> ProvideDiagnostics()
    {
        if (_statements is null)
        {
            return [];
        }

        Stack<InnerStatementNode> parents = [];
        PositionConverter converter = MakeConverter();
        DiagnosticContext context = new(ref converter);
        foreach (var child in _statements)
        {
            (child as IDiagnosticProvider)?.VerifySyntax(ref context);
            (child as IContextProvider)?.VerifyContext(ref context, _definedSymbols);

            if (child is InnerStatementNode parent)
                parents.Push(parent);
        }
        while (parents.TryPop(out var node))
        {
            foreach (var child in node.Children ?? [])
            {
                (child as IDiagnosticProvider)?.VerifySyntax(ref context);
                (child as IContextProvider)?.VerifyContext(ref context, _definedSymbols);

                if (child is InnerStatementNode parent)
                    parents.Push(parent);
            }
        }
        foreach (var unexpectedToken in _unexpectedTokens)
            context.AddError("Unexpected token", unexpectedToken);

        return context.Diagnostics;
    }

    public CompletionList ProvideCompletions(Position pos)
    {
        var offset = ProtocolToLocal(pos);
        List<CompletionItem> result = [];
        if (GetToken(offset) is not { } token)
            return new CompletionList();

        var prefix = source.AsSpan(token.start, offset - token.start);

        foreach (var (id, _) in _definedSymbols)
        {
            var start = id.start;
            var length = id.length;
            var label = source.AsSpan(start, length);
            if (!label.StartsWith(prefix))
                continue;

            result.Add(new CompletionItem
            {
                Label = source.Substring(start, length),
                Kind = CompletionItemKind.Function
            });
        }

        foreach (var (kw, _) in Grammar.Keywords.Dictionary)
        {
            if (!kw.AsSpan().StartsWith(prefix))
                continue;

            result.Add(new CompletionItem
            {
                Label = kw,
                Kind = CompletionItemKind.Keyword
            });
        }

        foreach (var (unit, _) in Grammar.UnitLiterals.Dictionary)
        {
            if (!unit.AsSpan().StartsWith(prefix))
                continue;

            result.Add(new CompletionItem
            {
                Label = unit,
                Kind = CompletionItemKind.Keyword
            });
        }

        foreach (var (label, (kind, _)) in Grammar.DefinedSymbols.Dictionary)
        {
            if (!label.AsSpan().StartsWith(prefix))
                continue;

            var cik = kind switch
            {
                SymbolKind.Undefined => CompletionItemKind.Text,
                SymbolKind.Feature => CompletionItemKind.Class,
                SymbolKind.Function => CompletionItemKind.Function,
                SymbolKind.Variable => CompletionItemKind.Variable,
                SymbolKind.Parameter => CompletionItemKind.Variable,
                SymbolKind.Constant => CompletionItemKind.Constant,
                _ => (CompletionItemKind)0
            };
            if (cik == 0)
                continue;

            result.Add(new CompletionItem
            {
                Label = label,
                Kind = cik
            });
        }

        return new CompletionList { Items = result, IsIncomplete = true };
    }
}