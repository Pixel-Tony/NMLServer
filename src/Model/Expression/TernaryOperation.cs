using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class TernaryOperation : ExpressionAST
{
    private ExpressionAST? _condition;
    public ExpressionAST? TrueBranch;
    public ExpressionAST? FalseBranch;
    public TernaryOpToken QuestionMark;
    public ColonToken? Colon;

    public const int Precedence = Grammar.TernaryOperatorPrecedence;

    public TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : base(parent)
    {
        QuestionMark = questionMark;
    }

    public TernaryOperation(ExpressionAST? parent, ExpressionAST? condition, TernaryOpToken questionMark)
        : this(parent, questionMark)
    {
        _condition = condition;
    }

    protected override void Replace(ExpressionAST target, ExpressionAST value)
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