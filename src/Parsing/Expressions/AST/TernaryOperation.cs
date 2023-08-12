using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class TernaryOperation : ExpressionAST, IHasPrecedence
{
    private readonly ExpressionAST? _condition;
    public TernaryOpToken QuestionMark;
    public ExpressionAST? TrueBranch;
    public ColonToken? Colon;
    public ExpressionAST? FalseBranch;

    public const int Precedence = 1;
    public int precedence => Precedence;

    public TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : base(parent)
    {
        QuestionMark = questionMark;
    }

    public TernaryOperation(ExpressionAST? parent, ExpressionAST? condition, TernaryOpToken questionMark) 
        : this(parent, questionMark)
    {
        _condition = condition;
    }

    public override string ToString() => $"({_condition} ? {TrueBranch} {(Colon != null ? ':' : '.')} {FalseBranch})";
}