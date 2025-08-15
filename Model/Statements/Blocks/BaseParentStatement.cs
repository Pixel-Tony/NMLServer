using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;

namespace NMLServer.Model.Statements.Blocks;

internal abstract class BaseParentStatement : BaseBlockStatement
{
    public List<BaseStatement>? Children;

    public BaseParentStatement(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is null)
            Children = [];
    }

    protected sealed override int? MiddleEnd => Children?[^1].End;

    public override void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
        => IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, ranges, ref converter, true);

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("ParentStmt");
        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}