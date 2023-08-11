using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class TernaryOperation : ExpressionAST
{
    public ExpressionAST? Condition;
    public ExpressionAST? TrueBranch;
    public ExpressionAST? FalseBranch;
    public TernaryOpToken? QuestionMark;
    public ColonToken? Colon;

    public TernaryOperation(ExpressionAST? parent, TernaryOpToken? questionMark) : base(parent)
    {
        QuestionMark = questionMark;
    }

    public override string ToString() => $"({Condition} ? {TrueBranch} : {FalseBranch})";
}