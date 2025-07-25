#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using DotNetGraph.Extensions;
#endif
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal abstract partial class BaseSwitch(
    ref ParsingState state,
    KeywordToken keyword,
    BlockStatement.ParamInfo info)
    : BlockStatement<BaseSwitch.Line>(ref state, keyword, info)
{
#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel("Switch");
#endif
}