using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

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

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "UnitExpr").WithExprStyle();
        child.MaybeVisualize(graph, n, ctx);
        return n;
    }
}