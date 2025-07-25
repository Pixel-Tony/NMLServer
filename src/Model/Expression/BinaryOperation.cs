#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class BinaryOperation(ExpressionAST? parent, BinaryOpToken op) : ExpressionAST(parent)
{
    public ExpressionAST? Left;
    public ExpressionAST? Right;
    public OperatorType OperatorType => op.Type;

    public override int Start => Left?.Start ?? op.Start;

    public override int End => Right?.End ?? op.End;

    public uint Precedence => op.Precedence;

    public BinaryOperation(ExpressionAST? parent, ExpressionAST? lhs, BinaryOpToken op) : this(parent, op)
    {
        Left = lhs;
    }

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        if (Left == target)
        {
            Left = value;
            return;
        }
        Right = value;
    }

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (Left is not null)
            Left.VerifySyntax(in context);
        else if (OperatorType is not (OperatorType.Plus or OperatorType.Minus))
            context.Add(Errors.ErrorMissingExpr, int.Max(op.Start - 1, 0));

        if (Right is null)
            context.Add(Errors.ErrorMissingExpr, op.End);
        else
            Right.VerifySyntax(in context);
    }

#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "BinaryOp")
            .WithExprStyle();

        if (Left is not null || OperatorType is not (OperatorType.Plus or OperatorType.Minus))
            Left.MaybeVisualize(graph, n, ctx);

        op.Visualize(graph, n, ctx);
        Right.MaybeVisualize(graph, n, ctx);

        return n;
    }
#endif
}