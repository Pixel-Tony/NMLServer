using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace NMLServer.Model.Statements.Blocks;

internal class FlowControlBlock(ref ParsingState state, KeywordToken keyword) : BaseParentStatement(ref state, keyword)
{
    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel(Keyword.Keyword.ToString());
}