using NMLServer.Lexing;
using NMLServer.Model;
using NMLServer.Model.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

using DefinitionsMap = Dictionary<IdentifierToken, List<IdentifierToken>>;

internal partial class Document
{
    private string _source;
    private int _version;

    private List<Token> _tokens = [];
    private List<StatementAST>? _statements = [];
    private readonly List<int> _lineLengths = [];
    private readonly DefinitionsMap _definedSymbols;
    private readonly IdentifierComparer _symbolComparer;
    private readonly List<Token> _unexpectedTokens = [];

    public readonly TextDocumentAttributes Attributes;
    public readonly DocumentUri Uri;

    private string source
    {
        get => _source;
        set
        {
            _source = value;
            _symbolComparer.context = value;
        }
    }

    public PositionConverter GetConverter() => new(_lineLengths);

    public Document(TextDocumentItem item)
    {
        Attributes = new TextDocumentAttributes(item.Uri, item.LanguageId);
        Uri = item.Uri;
        _version = item.Version ?? 0;
        _source = item.Text;
        _symbolComparer = new IdentifierComparer(_source);
        _definedSymbols = new DefinitionsMap(_symbolComparer);
        MakeTokensFromScratch();
        MakeStatementsFromScratch();
        MakeDefinitionsFromScratch();
    }

    // TODO: full -> incremental; extract to handler
    public void ProvideSemanticTokens(SemanticTokensBuilder builder)
    {
        var converter = GetConverter();
        foreach (var token in _tokens)
        {
            if (!Grammar.GetTokenSemanticType(token, out var type))
            {
                continue;
            }
            // only comments can span multiple lines
            if (token is not CommentToken)
            {
                var (line, @char) = converter.LocalToProtocol(token.start);
                builder.Push(line, @char, token.length, type);
                continue;
            }
            foreach (var (line, @char, length) in converter.LocalToProtocol(token.start, token.length))
            {
                builder.Push(line, @char, length, type);
            }
        }
    }

    public IReadOnlyList<IdentifierToken>? GetDefinitions(IdentifierToken symbol)
    {
        _definedSymbols.TryGetValue(symbol, out var definitions);
        return definitions;
    }

    public Token? GetToken(Position pos) => GetTokenAt(ProtocolToLocal(pos));

    private Token? GetTokenAt(int coordinate)
    {
        for (int left = 0, right = _tokens.Count - 1; left <= right;)
        {
            int mid = left + (right - left) / 2;
            var current = _tokens[mid];
            if (coordinate < current.start)
            {
                right = mid - 1;
                continue;
            }
            if (coordinate <= current.end)
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
        {
            start += _lineLengths[i];
        }
        return start;
    }

    private void MakeTokensFromScratch()
    {
        _tokens.Clear();
        _lineLengths.Clear();
        Lexer lexer  = new (source, in _lineLengths);
        lexer.ProcessUntilFileEnd(in _tokens);
    }

    private void MakeStatementsFromScratch()
    {
        _unexpectedTokens.Clear();
        ParsingState state = new(_tokens, in _unexpectedTokens);
        _statements = StatementAST.Build(ref state);
    }

    private void MakeDefinitionsFromScratch()
    {
        _definedSymbols.Clear();
        if (_statements is null)
        {
            return;
        }
        foreach (var child in _statements)
        {
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