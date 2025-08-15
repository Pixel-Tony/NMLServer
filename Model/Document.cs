using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using NMLServer.Model.Syntax;
using NMLServer.Model.Grammar;
using NMLServer.Model.Processors;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;

namespace NMLServer.Model;

internal sealed class Document
{
    public readonly DocumentUri Uri;
    public int Version { get; private set; }
    public ref AbstractSyntaxTree AST => ref _AST;
    private AbstractSyntaxTree _AST;

    public readonly DefinitionProcessor Definitions = new();
    public readonly FoldingRangeProcessor FoldingRanges = new();
    public readonly DocumentSymbolProcessor DocumentSymbols = new();
    public readonly DiagnosticProcessor Diagnostics = new();

    private readonly IIncrementalNodeProcessor[] _processorsPipeline;

    public Document(TextDocumentItem item)
    {
        Uri = item.Uri;
        Version = item.Version;
        _processorsPipeline = [
            FoldingRanges,
            Definitions,
            DocumentSymbols,
            Diagnostics
        ];
        _AST = new AbstractSyntaxTree(item.Text);
        TreeTraverser traverser = new(_AST);
        ProcessChangedSyntax(ref traverser, null);
    }

    // TODO
    public void ProvideCompletions(List<CompletionItem> result)
    {
        foreach (var (label, locations) in Definitions.Symbols)
            result.Add(new() { Label = label, Kind = NML.GetCompletionItemKind(locations[0].kind) });
    }

    public void AcceptChanges(int newVersion, List<TextDocumentContentChangeEvent> changes)
    {
        if (newVersion <= Version)
            return;
        foreach (var change in changes)
        {
            var end = _AST.Amend(change.Range, change.Text, out var traverser);
            ProcessChangedSyntax(ref traverser, end);
        }
        Version = newVersion;
    }

    private void ProcessChangedSyntax(ref TreeTraverser traverser, BaseStatement? end)
    {
        StringView source = _AST.Tokens.Source;
        var converter = _AST.Tokens.MakeConverter();

        // Since some features depend on the others (a.e. semantics depend on the definitions even from next nodes),
        // we cannot call all the processors at once for every new node, so we use a pipeline
        foreach (var processor in _processorsPipeline)
        {
            processor.Trim();
            TreeTraverser trv = new(traverser);
            foreach (var (parent, _) in trv.Navigation)
            {
                NodeProcessingContext context = new(source, ref converter, true);
                processor.Process(parent, context);
            }
            for (; trv.Current is { } node && node != end; trv.Increment())
            {
                var isParent = node is BaseParentStatement { Children: { } };

                NodeProcessingContext context = new(source, ref converter, isParent);
                processor.Process(node, context);
            }
            processor.FinishIncrement(ref _AST);
        }
    }
}