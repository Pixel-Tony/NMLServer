using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;

namespace NMLServer.Model.Expressions;

internal sealed class UnaryOperation(BaseExpression? parent, UnaryOpToken operation) : BaseExpression(parent)
{
    public BaseExpression? Expression;

    public override int Start => operation.Start;

    public override int End => Expression?.End ?? operation.End;

    protected override void Replace(BaseExpression target, FunctionCall value)
    {
        Expression = value;
    }

    public override void VerifySyntax(DiagnosticContext context)
    {
        if (Expression is null)
        {
            context.Add(ErrorStrings.ErrorMissingExpr, int.Max(Start - 1, 0));
            return;
        }
        Expression.VerifySyntax(context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "UnaryOperation").WithExprStyle();
        Expression.MaybeVisualize(graph, n, ctx);
        return n;
    }
}