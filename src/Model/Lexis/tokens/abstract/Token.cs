using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Lexis;

internal abstract class Token(int start, int length) : IHasStart, IHasEnd, IVisualProvider
{
    public int Start { get; set; } = start;

    public int End => Start + Length;

    public int Length { get; } = length;

    internal abstract string? SemanticType { get; }

    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => VizExtensions.MakeNode(graph, parent, ctx[Start..End])
            .WithTokenFeatures();
}