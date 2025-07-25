#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif
using NMLServer.Model.Expression;

namespace NMLServer.Model.Lexis;

internal abstract class Token(int start, int length) : IHasStart, IHasEnd, IVisualProvider
{
    public int Start { get; set; } = start;

    public int End => Start + Length;

    public int Length { get; } = length;

    public abstract string? SemanticType { get; }

#if TREE_VISUALIZER_ENABLED
    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => VizExtensions.MakeNode(graph, parent, ctx[Start..End])
            .WithTokenFeatures();
#endif
}