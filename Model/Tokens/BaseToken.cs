using DotNetGraph.Core;
using NMLServer.Extensions;

namespace NMLServer.Model.Tokens;

internal abstract class BaseToken(int start, int length) : IHasStart, IHasEnd, IVisualProvider
{
    public int Start { get; set; } = start;

    public int End => Start + Length;

    public int Length { get; } = length;

    public abstract string? SemanticType { get; }

    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => VizExtensions.MakeNode(graph, parent, ctx[Start..End])
            .WithTokenFeatures();

    public virtual StringView Context(StringView source) => source.Slice(Start, Length);
}