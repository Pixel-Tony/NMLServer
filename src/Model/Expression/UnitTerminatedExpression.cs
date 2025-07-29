using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Expression;

internal sealed class UnitTerminatedExpression(ExpressionAST? child, UnitToken token) : ExpressionAST(null)
{
    public override int Start => child?.Start ?? token.Start;
    public override int End => token.End;

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (child is null)
        {
            context.Add(Errors.ErrorMissingExpr, int.Max(Start - 1, 0));
            return;
        }
        child.VerifySyntax(in context);
    }

#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "UnitExpr").WithExprStyle();
        child.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}