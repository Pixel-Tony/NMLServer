using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.PublishDiagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using NMLServer.Extensions;
using NMLServer.Model.Grammar;
using NMLServer.Model.Processors;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;

namespace NMLServer.Model;

using ParentStack = Stack<(IReadOnlyList<BaseStatement> nodes, int index)>;

internal sealed class Document
{
    public readonly DocumentUri Uri;
    public int Version { get; private set; }
    public ref AbstractSyntaxTree AST => ref _AST;
    private AbstractSyntaxTree _AST;

    public readonly DefinitionProcessor Definitions = new();
    public readonly FoldingRangeProcessor FoldingRanges = new();

    private readonly IIncrementalNodeProcessor[] _processorsPipeline;

    public Document(TextDocumentItem item)
    {
        Uri = item.Uri;
        Version = item.Version;
        _processorsPipeline = [Definitions, FoldingRanges];
        _AST = new AbstractSyntaxTree(item.Text);
        ProcessChangedSyntax((null, null));
    }

    public void AcceptChanges(int newVersion, List<TextDocumentContentChangeEvent> changes)
    {
        if (newVersion <= Version)
            return;
        foreach (var change in changes)
        {
            var text = change.Text;
            var range = _AST.Amend(change.Range, text);
            ProcessChangedSyntax(range);
        }
        Version = newVersion;
    }

    // TODO
    public void ProvideCompletions(ref readonly List<CompletionItem> result)
    {
        foreach (var (label, tokens) in Definitions.Symbols)
            result.Add(new() { Label = label, Kind = NML.GetCompletionItemKind(tokens[0].Kind) });
    }

    // TODO
    private void ProcessChangedSyntax((BaseStatement? start, BaseStatement? end) range)
    {
#if DEBUG
        List<Diagnostic> info = [];
#endif
        StringView source = _AST.Tokens.Source;
        var navigationStack = GetNavigationStackForNode(range.start);
        var reversedNavigationStack = new ParentStack(navigationStack);
        var converter = _AST.Tokens.MakeConverter();
        (int start, int end) replacedRange = (range.start?.Start ?? 0, range.end?.End ?? source.Length);
        Range replacedProtocolRange;
        {
            var copy = converter.Copy();
            var start = copy.LocalToProtocol(replacedRange.start);
            var end = copy.LocalToProtocol(replacedRange.end);
            replacedProtocolRange = new(start, end);
        }

        // Since some features depend on the others (a.e. semantics depend on the definitions even from next nodes),
        // we cannot call all the processors at once for every new node, so we use a pipeline
        foreach (var processor in _processorsPipeline)
        {
            processor.Trim(replacedRange, replacedProtocolRange);
            ParentStack stack = new(reversedNavigationStack);

            while (stack.TryPop(out var pair))
            {
                var (children, i) = pair;
                for (var max = children.Count; i < max;)
                {
                    var node = children[i];
                    if (node == range.end)
                        return;

                    bool isParent = false;
                    if (node is BaseParentStatement { Children: { } grandChildren })
                    {
                        isParent = true;
                        stack.Push((children, i + 1));
                        children = grandChildren;
                        max = children.Count;
                        i = -1;
                    }
                    ++i;

                    var nodeStart = converter.LocalToProtocol(node.Start);
                    var nodeEnd = isParent
                        ? converter.Copy().LocalToProtocol(node.End)
                        : converter.LocalToProtocol(node.End);

                    NodeProcessingContext context = new(source, isParent, ref converter);
                    processor.Process(node, context);

                    // TODO remove
#if DEBUG
                    info.Add(new Diagnostic()
                    {
                        Severity = DiagnosticSeverity.Information,
                        Message = "ProcessChangedSyntax",
                        Range = new Range(nodeStart, nodeEnd)
                    });
#endif
                }
            }
        }

#if DEBUG
        Program.Server.Client.PublishDiagnostics(
            new PublishDiagnosticsParams() { Diagnostics = info, Uri = Uri, Version = Version }
        ).Wait();
#endif
    }

    private ParentStack GetNavigationStackForNode(BaseStatement? node)
    {
        ParentStack stack = [];
        var offset = node?.Start ?? 0;
        for (var children = _AST.Nodes; ;)
        {
            var childIndex = children.FindFirstNotBefore(offset);
            var child = children[childIndex];
            if (child == node || child is not BaseParentStatement { Children: { } grandChildren })
            {
                stack.Push((children, childIndex));
                break;
            }
            stack.Push((children, childIndex + 1));
            children = grandChildren;
        }
        return stack;
    }
}