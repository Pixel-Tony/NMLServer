using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : ExpressionAST(parent)
{
    public const int Precedence = Grammar.TernaryOperatorPrecedence;

    private ExpressionAST? _condition;
    public ExpressionAST? TrueBranch;
    public ColonToken? Colon;
    public ExpressionAST? FalseBranch;

    public override int start => _condition?.start ?? questionMark.start;

    public override int end => FalseBranch?.end ?? Colon?.end ?? TrueBranch?.end ?? questionMark.end;

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
}