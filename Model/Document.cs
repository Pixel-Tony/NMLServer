using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using NMLServer.Model.Syntax;
using NMLServer.Model.Grammar;
using NMLServer.Model.Processors;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Statements;

namespace NMLServer.Model;

internal sealed class Document
{
    public readonly DocumentUri Uri;
    public int Version { get; private set; }
    public ref AbstractSyntaxTree AST => ref _AST;
    private AbstractSyntaxTree _AST;

    public readonly FoldingRangeProcessor FoldingRanges = new();
    public readonly DefinitionProcessor Definitions = new();
    public readonly DiagnosticProcessor Diagnostics = new();

    public Document(TextDocumentItem item)
    {
        Uri = item.Uri;
        Version = item.Version;
        _AST = new AbstractSyntaxTree(item.Text);
        TreeTraverser traverser = new(in _AST);
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
        // Since some features depend on the others (a.e. semantic tokens depend on the definitions even from next nodes),
        // we cannot call all the processors at once for every new node, so we use a pipeline
        ReadOnlySpan<IIncrementalNodeProcessor> pipeline = [
            FoldingRanges,
            Definitions,
            Diagnostics
        ];
        IncrementContext context = new(_AST.Tokens.Source, ref _AST);
        foreach (var processor in pipeline)
        {
            TreeTraverser trv = new(traverser);
            processor.ProcessChangedSyntax(ref trv, end, ref context);
        }
    }
}