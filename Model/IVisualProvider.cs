using DotNetGraph.Core;

namespace NMLServer.Model;

internal interface IVisualProvider
{
    public DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx);
}