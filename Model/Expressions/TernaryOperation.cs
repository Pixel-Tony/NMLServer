using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Expressions;

internal sealed class TernaryOperation(BaseExpression? parent, TernaryOpToken questionMark) : BaseExpression(parent)
{
    private BaseExpression? _condition;
    public BaseExpression? TrueBranch;
    public ColonToken? Colon;
    public BaseExpression? FalseBranch;

    public override int Start => _condition?.Start ?? questionMark.Start;

    public override int End => FalseBranch?.End ?? Colon?.End ?? TrueBranch?.End ?? questionMark.End;

    public const int Precedence = ((int)OperatorType.QuestionMark) >> 8;

    public TernaryOperation(BaseExpression? parent, BaseExpression? condition, TernaryOpToken questionMark)
        : this(parent, questionMark)
    {
        _condition = condition;
    }

    protected override void Replace(BaseExpression target, FunctionCall value)
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

    public override void VerifySyntax(DiagnosticContext context)
    {
        // TODO
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "TernaryOp").WithExprStyle();
        _condition.MaybeVisualize(graph, n, ctx);
        questionMark.Visualize(graph, n, ctx);
        TrueBranch.MaybeVisualize(graph, n, ctx);
        Colon.MaybeVisualize(graph, n, ctx);
        FalseBranch.MaybeVisualize(graph, n, ctx);
        return n;
    }
}