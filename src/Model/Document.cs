using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.ShowMessage;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using NMLServer.Model.Diagnostics;
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
        _tokens = new TokenStorage(item.Text);
        (_statements, _unexpectedTokens) = MakeStatements();
        _definedSymbols = MakeDefinitions();
    }

    public bool Has(IdentifierToken token, [NotNullWhen(true)] out List<IdentifierToken>? definitions)
        => _definedSymbols.GetAlternateLookup<StringView>()
            .TryGetValue(_tokens.GetSymbolContext(token), out definitions);

    public List<Location>? TryGetDefinitionLocations(Position position)
    {
        if (_tokens.TryGetAt(position) is not IdentifierToken symbol)
            return null;

        if (!_definedSymbols.GetAlternateLookup<StringView>()
                .TryGetValue(_tokens.GetSymbolContext(symbol), out var definitions))
            return null;

        List<Location> locations = new(definitions.Count);
        var length = symbol.Length;
        var converter = _tokens.MakeConverter();
        foreach (var definition in definitions)
        {
            var definitionStart = definition.Start;
            var start = converter.LocalToProtocol(definitionStart);
            var end = start with { Character = start.Character + length };
            locations.Add(new Location(Uri, new Range(start, end)));
        }
        return locations;
    }

    public List<Diagnostic> ProvideDiagnostics()
    {
        if (_statements.Count == 0)
            return [];

        Stack<InnerStatementNode> parents = [];
        var converter = _tokens.MakeConverter();
        DiagnosticContext context = new(ref converter);
        foreach (var child in _statements)
            SupplyDiagnostics(child, ref context);
        while (parents.TryPop(out var node))
            if (node.Children is { } children)
                foreach (var child in children)
                    SupplyDiagnostics(child, ref context);
        foreach (var unexpectedToken in _unexpectedTokens)
            context.Add("Unexpected token", unexpectedToken);

        return context.Diagnostics;

        void SupplyDiagnostics(StatementAST child, ref DiagnosticContext context)
        {
            (child as IDiagnosticProvider)?.VerifySyntax(ref context);
            (child as IContextProvider)?.VerifyContext(ref context, this);
            if (child is InnerStatementNode parent)
                parents.Push(parent);
        }
    }

    public CompletionList ProvideCompletions(Position position)
    {
        List<CompletionItem> result = [];
        var prefix = _tokens.GetPrefix(position);
        if (prefix == StringView.Empty)
            return new CompletionList();

        foreach (var id in _definedSymbols.Keys)
            if (id.AsSpan().StartsWith(prefix))
                result.Add(new CompletionItem { Label = id, Kind = CompletionItemKind.Function });
        foreach (var kw in Grammar.Keywords.Dictionary.Keys)
            if (kw.AsSpan().StartsWith(prefix))
                result.Add(new CompletionItem { Label = kw, Kind = CompletionItemKind.Keyword });
        foreach (var unit in Grammar.UnitLiterals.Dictionary.Keys)
            if (unit.AsSpan().StartsWith(prefix))
                result.Add(new CompletionItem { Label = unit, Kind = CompletionItemKind.Keyword });

        foreach (var (label, kind) in Grammar.DefinedSymbols.Dictionary)
        {
            if (!label.AsSpan().StartsWith(prefix))
                continue;
            var cik = kind switch
            {
                SymbolKind.Feature => CompletionItemKind.Class,
                SymbolKind.Function => CompletionItemKind.Function,
                SymbolKind.Variable => CompletionItemKind.Variable,
                SymbolKind.Parameter => CompletionItemKind.Variable,
                SymbolKind.Constant => CompletionItemKind.Constant,
                _ => (CompletionItemKind)0
            };
            if (cik != 0)
                result.Add(new CompletionItem { Label = label, Kind = cik });
        }

        return new CompletionList { IsIncomplete = true, Items = result };
    }

    public void ProvideSemanticTokens(in SemanticTokensBuilder builder)
        => _tokens.ProvideSemanticTokens(in builder, this);

    public void ProvideSemanticTokens(in SemanticTokensBuilder builder, Range range)
        => _tokens.ProvideSemanticTokens(in builder, range, this);

    public void Handle(DidChangeTextDocumentParams request)
    {
        int newVersion = request.TextDocument.Version;
        if (newVersion <= Version)
            return;

        foreach (var change in request.ContentChanges)
        {
            var text = change.Text;
            if (change.Range is { } replacedRange)
                _tokens.Rebuild(replacedRange, text);
            else
                _tokens = new TokenStorage(text);
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

    public void Visualize()
    {
        _visualizationTimer ??= new Timer(_ => VisualizeAsync().Wait());
        _visualizationTimer.Change(TimeSpan.FromMilliseconds(300), Timeout.InfiniteTimeSpan);
    }

    private async Task VisualizeAsync()
    {
        var graph = new DotGraph().WithIdentifier("MyGraph")
            .WithAttribute("bgcolor", "transparent")
            .WithAttribute("dpi", "400");
        var root = new DotNode().WithIdentifier("Root")
            .WithAttribute("fontname", "Consolas");
        graph.Add(root);

        foreach (var child in _statements)
            child.Visualize(graph, root, _tokens.Source);

        if (graph.Elements.Count >= 400)
        {
            const string msg = "Graph too big, visualization aborted.";
            await Program.Server.Client.ShowMessage(new ShowMessageParams
                { Message = msg, Type = MessageType.Debug });
            return;
        }
        await using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        await graph.CompileAsync(context).ConfigureAwait(false);

        var result = writer.GetStringBuilder().ToString();
        await File.WriteAllTextAsync("graph.dot", result).ConfigureAwait(false);
        await Process.Start("dot", "-Tpng -ograph.png graph.dot").WaitForExitAsync().ConfigureAwait(false);
    }

    public void Dispose() => _visualizationTimer?.Dispose();

    private (List<StatementAST> root, List<Token> unexpectedTokens) MakeStatements()
    {
        ParsingState state = new(_tokens.List);
        StatementASTBuilder builder = new();
        while (builder.Make(ref state))
        { }
        return (builder.Root, state.UnexpectedTokens);
    }

    private DefinitionsMap MakeDefinitions()
    {
        DefinitionsMap map = new();
        if (_statements.Count == 0)
            return map;

        Stack<InnerStatementNode> parents = [];
        foreach (var child in _statements)
            TryAddDefinition(child, parents, map, ref _tokens);
        while (parents.TryPop(out var node))
            if (node.Children is { } children)
                foreach (var child in children)
                    TryAddDefinition(child, parents, map, ref _tokens);
        return map;

        static void TryAddDefinition(StatementAST child, Stack<InnerStatementNode> parents, DefinitionsMap map,
            ref readonly TokenStorage tokens)
        {
            if (child is InnerStatementNode parent)
                parents.Push(parent);

            if (child is not ISymbolSource { Symbol: { } symbol })
                return;

            var context = tokens.GetSymbolContext(symbol);
            if (map.GetAlternateLookup<StringView>().TryGetValue(context, out var equalSymbols))
            {
                equalSymbols.Add(symbol);
                return;
            }
            map[new string(context)] = [symbol];
        }
    }

    private Timer? _visualizationTimer;
    private TokenStorage _tokens;
    private List<StatementAST> _statements;
    private List<Token> _unexpectedTokens;
    private DefinitionsMap _definedSymbols;
}