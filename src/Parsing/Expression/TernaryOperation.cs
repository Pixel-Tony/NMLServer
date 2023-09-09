using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class TernaryOperation : ExpressionAST, IHasPrecedence
{
    private ExpressionAST? _condition;
    public ExpressionAST? TrueBranch;
    public ExpressionAST? FalseBranch;
    public TernaryOpToken QuestionMark;
    public ColonToken? Colon;

    public int precedence => Grammar.TernaryOperatorPrecedence;

    public TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : base(parent)
    {
        QuestionMark = questionMark;
    }

    public TernaryOperation(ExpressionAST? parent, ExpressionAST? condition, TernaryOpToken questionMark) 
        : this(parent, questionMark)
    {
        _condition = condition;
    }

    public override void Replace(ExpressionAST target, ExpressionAST value)
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

    public override string ToString() => $"({_condition} ? {TrueBranch} {(Colon != null ? ':' : '.')} {FalseBranch})";
}