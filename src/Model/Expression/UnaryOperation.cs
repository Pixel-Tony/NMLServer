using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class UnaryOperation(ExpressionAST? parent, UnaryOpToken operation) : ExpressionAST(parent)
{
    public ExpressionAST? Expression;

    public override int start => operation.start;

    public override int end => Expression?.end ?? operation.end;

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        Expression = value;
    }

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (Expression is null)
        {
            context.Add(ErrorStrings.ErrorMissingExpr, int.Max(start - 1, 0));
            return;
        }
        Expression.VerifySyntax(in context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "UnaryOperation").WithExprStyle();
        Expression.MaybeVisualize(graph, n, ctx);
        return n;
    }
}