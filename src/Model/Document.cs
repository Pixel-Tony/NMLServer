using System.Diagnostics.CodeAnalysis;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using NMLServer.Model.Lexis;
using NMLServer.Model.Statement;

namespace NMLServer.Model;

using DefinitionsMap = Dictionary<string, List<IdentifierToken>>;

internal sealed class Document : IDefinitionsBag
{
    public readonly DocumentUri Uri;
    public int Version { get; private set; }

    public Document(TextDocumentItem item)
    {
        Uri = item.Uri;
        Version = item.Version;
        Tokens = new TokenStorage(item.Text);
        (_statements, _unexpectedTokens) = MakeStatements();
        _definedSymbols = MakeDefinitions();
    }

    public bool Has(IdentifierToken token, [NotNullWhen(true)] out List<IdentifierToken>? definitions)
        => DefinedSymbolsLookup.TryGetValue(Tokens.GetSymbolContext(token), out definitions);

    // TODO
    public void ProvideCompletions(ref readonly List<CompletionItem> result)
    {
        foreach (var (s, toks) in _definedSymbols)
            result.Add(new() { Label = s, Kind = Grammar.GetCompletionItemKind(toks[0].Kind) });
    }

    public void AcceptChanges(int newVersion, List<TextDocumentContentChangeEvent> changes)
    {
        if (newVersion <= Version)
            return;

        foreach (var change in changes)
        {
            var text = change.Text;
            if (change.Range is { } replacedRange)
                Tokens.Rebuild(replacedRange, text);
            else
                Tokens = new TokenStorage(text);
        }
        // TODO incremental
        {
            foreach (var v in _definedSymbols.Values)
                foreach (var token in v)
                    token.Kind = SymbolKind.Undefined;
            (_statements, _unexpectedTokens) = MakeStatements();
            _definedSymbols = MakeDefinitions();
        }
        Version = newVersion;
    }

    private (List<StatementAST> root, List<Token> unexpectedTokens) MakeStatements()
    {
        ParsingState state = new(Tokens.Items);
        StatementASTBuilder builder = new();
        while (builder.Make(ref state))
        { }
        return (builder.Root, state.UnexpectedTokens);
    }

    private DefinitionsMap MakeDefinitions()
    {
        DefinitionsMap map = [];
        var lookup = map.GetAlternateLookup<StringView>();
        if (_statements.Count == 0)
            return map;

        Stack<InnerStatementNode> parents = [];
        foreach (var child in _statements)
            TryAddDefinition(child, parents, map, lookup, ref Tokens);
        while (parents.TryPop(out var node))
            if (node.Children is { } children)
                foreach (var child in children)
                    TryAddDefinition(child, parents, map, lookup, ref Tokens);
        return map;

        static void TryAddDefinition(StatementAST child, Stack<InnerStatementNode> parents, DefinitionsMap map,
                DefinitionsMap.AlternateLookup<StringView> lookup,
            ref readonly TokenStorage tokens)
        {
            if (child is InnerStatementNode parent)
                parents.Push(parent);
            if (child is not ISymbolSource { Symbol: { } symbol })
                return;
            var context = tokens.GetSymbolContext(symbol);
            if (lookup.TryGetValue(context, out var equalSymbols))
            {
                equalSymbols.Add(symbol);
                return;
            }
            map[new string(context)] = [symbol];
        }
    }

    public TokenStorage Tokens;
    private List<StatementAST> _statements;
    public IReadOnlyList<StatementAST> Statements => _statements;
    private List<Token> _unexpectedTokens;
    public IReadOnlyList<Token> UnexpectedTokens => _unexpectedTokens;
    private DefinitionsMap _definedSymbols;
    public DefinitionsMap.AlternateLookup<StringView> DefinedSymbolsLookup => _definedSymbols.GetAlternateLookup<StringView>();
}