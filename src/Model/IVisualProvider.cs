#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
#endif

namespace NMLServer.Model;

internal interface IVisualProvider
{
#if TREE_VISUALIZER_ENABLED
    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx);
#endif
}