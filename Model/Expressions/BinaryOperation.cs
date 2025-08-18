using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Expressions;

internal sealed class BinaryOperation(BaseExpression? parent, BinaryOpToken op) : BaseExpression(parent)
{
    public BaseExpression? Left;
    public BaseExpression? Right;
    public readonly BinaryOpToken Operator = op;

    public OperatorType OperatorType => Operator.Type;

    public override int Start => Left?.Start ?? Operator.Start;

    public override int End => Right?.End ?? Operator.End;

    public int Precedence => Operator.Type.Precedence();

    public BinaryOperation(BaseExpression? parent, BaseExpression? lhs, BinaryOpToken op) : this(parent, op)
    {
        Left = lhs;
    }

    protected override void Replace(BaseExpression target, FunctionCall value)
    {
        if (Left == target)
        {
            Left = value;
            return;
        }
        Right = value;
    }

    public override void VerifySyntax(DiagnosticContext context)
    {
        if (Left is not null)
            Left.VerifySyntax(context);
        else if (OperatorType is not (OperatorType.Plus or OperatorType.Minus))
            context.Add(ErrorStrings.Err_MissingExpression, int.Max(Operator.Start - 1, 0));

        if (Right is null)
            context.Add(ErrorStrings.Err_MissingExpression, Operator.End);
        else
            Right.VerifySyntax(context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "BinaryOp")
            .WithExprStyle();

        if (Left is not null || OperatorType is not (OperatorType.Plus or OperatorType.Minus))
            Left.MaybeVisualize(graph, n, ctx);

        Operator.Visualize(graph, n, ctx);
        Right.MaybeVisualize(graph, n, ctx);

        return n;
    }
}