using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Lexis;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Statement;

internal abstract class InnerStatementNode : BlockStatement
{
    // TODO remove if not used, previously used only while constructing ast, can be replaced with a stack
    public readonly InnerStatementNode? Parent;
    public readonly List<StatementAST>? Children;

    protected sealed override int? MiddleEnd => Children?[^1].End;

    protected InnerStatementNode(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword,
        ParamInfo info) : base(ref state, keyword, info)
    {
        Parent = parent;
        if (OpeningBracket is not null && ClosingBracket is null)
        {
            Children = [];
        }
    }

    public override void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children,
        in List<FoldingRange> ranges, ref TokenStorage.PositionConverter converter)
    {
        IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, in ranges, ref converter, true);
        if (Children is null)
            return;
        foreach (var child in Children)
            if (child is IFoldingRangeProvider provider)
                children.Push(provider);
    }

#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx)
            .WithLabel("Node");

        foreach (var child in Children ?? [])
            child.Visualize(graph, n, ctx);

        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}