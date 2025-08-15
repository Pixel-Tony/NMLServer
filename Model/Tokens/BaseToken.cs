using DotNetGraph.Core;
using NMLServer.Extensions;

namespace NMLServer.Model.Tokens;

internal abstract class BaseToken(int start, int length) : IHasBounds, IVisualProvider
{
    public int Start { get; set; } = start;
    public readonly int Length = length;
    public int End => Start + Length;

    public abstract string? SemanticType { get; }

    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => VizExtensions.MakeNode(graph, parent, ctx[Start..End])
            .WithTokenFeatures();

    public StringView Context(StringView source) => source.Slice(Start, Length);
}