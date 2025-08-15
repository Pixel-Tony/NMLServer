using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model;

namespace NMLServer.Extensions;

internal static class VizExtensions
{
    private static int _nodeId = 1;

    public static DotNode MakeNode(DotGraph graph, DotNode parent, string label)
    {
        var n = new DotNode()
            .WithIdentifier($"id{_nodeId++}")
            .WithLabel(label)
            .WithAttribute("fontname", "Consolas")
            .WithStyle(DotNodeStyle.Filled);
        graph.Add(n);
        graph.Add(new DotEdge().From(parent).To(n).WithPenWidth(2));
        return n;
    }

    public static DotNode WithTokenFeatures(this DotNode n) => n.WithFillColor(DotColor.PaleGreen);

    public static DotNode WithExprStyle(this DotNode n) => n.WithFillColor(DotColor.PaleGoldenrod);

    public static DotNode WithStmtFeatures(this DotNode n) => n.WithFillColor(DotColor.LightBlue);

    private static void WithNullStyle(this DotNode n) => n.WithFillColor(DotColor.PaleVioletRed);

    public static void MaybeVisualize<T>(this T? node, DotGraph graph, DotNode parent, string ctx)
        where T : IVisualProvider
    {
        if (node is null)
        {
            MakeNode(graph, parent, ".").WithNullStyle();
            return;
        }
        node.Visualize(graph, parent, ctx);
    }
}
