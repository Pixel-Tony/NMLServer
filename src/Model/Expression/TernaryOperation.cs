#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : ExpressionAST(parent)
{
    public const int Precedence = Grammar.TernaryOperatorPrecedence;

    private ExpressionAST? _condition;
    public ExpressionAST? TrueBranch;
    public ColonToken? Colon;
    public ExpressionAST? FalseBranch;

    public override int Start => _condition?.Start ?? questionMark.Start;

    public override int End => FalseBranch?.End ?? Colon?.End ?? TrueBranch?.End ?? questionMark.End;

    public TernaryOperation(ExpressionAST? parent, ExpressionAST? condition, TernaryOpToken questionMark)
        : this(parent, questionMark)
    {
        _condition = condition;
    }

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        if (target == _condition)
        {
            _condition = value;
        }
        else if (target == TrueBranch)
        {
            TrueBranch = value;
        }
        else
        {
            FalseBranch = value;
        }
    }

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        // TODO
    }

#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "TernaryOp").WithExprStyle();

        _condition.MaybeVisualize(graph, n, ctx);
        questionMark.Visualize(graph, n, ctx);
        TrueBranch.MaybeVisualize(graph, n, ctx);
        Colon.MaybeVisualize(graph, n, ctx);
        FalseBranch.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}