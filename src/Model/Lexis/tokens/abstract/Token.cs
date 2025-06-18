using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Lexis;

internal abstract class Token(int start, int length) : IHasStart, IHasEnd, IVisualProvider
{
    public int start { get; set; } = start;

    public int end => start + length;

    public int length { get; } = length;

    internal abstract string? semanticType { get; }

    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => VizExtensions.MakeNode(graph, parent, ctx[start..end])
            .WithTokenFeatures();
}