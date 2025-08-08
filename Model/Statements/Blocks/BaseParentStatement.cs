using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;

namespace NMLServer.Model.Statements.Blocks;

internal abstract class BaseParentStatement(BaseParentStatement? parent, ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement(ref state, keyword)
{
    public readonly BaseParentStatement? Parent = parent;
    public List<BaseStatement>? Children = [];

    protected sealed override int? MiddleEnd => Children?[^1].End;

    public override void ProvideFoldingRanges(in List<FoldingRange> ranges, ref readonly PositionConverter converter)
        => IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, in ranges, in converter, true);

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Node");
        foreach (var child in Children ?? [])
            child.Visualize(graph, n, ctx);
        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}