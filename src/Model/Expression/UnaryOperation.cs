using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Expression;

internal sealed class UnaryOperation(ExpressionAST? parent, UnaryOpToken operation) : ExpressionAST(parent)
{
    public ExpressionAST? Expression;

    public override int Start => operation.Start;

    public override int End => Expression?.End ?? operation.End;

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        Expression = value;
    }

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (Expression is null)
        {
            context.Add(Errors.ErrorMissingExpr, int.Max(Start - 1, 0));
            return;
        }
        Expression.VerifySyntax(in context);
    }

#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "UnaryOperation").WithExprStyle();
        Expression.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}