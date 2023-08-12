using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class TernaryOperation : ExpressionAST
{
    public ExpressionAST? Condition;
    public TernaryOpToken QuestionMark;
    public ExpressionAST? TrueBranch;
    public ColonToken? Colon;
    public ExpressionAST? FalseBranch;

    public TernaryOperation(ExpressionAST? parent, TernaryOpToken questionMark) : base(parent)
    {
        QuestionMark = questionMark;
    }

    public override string ToString() => $"({Condition} ? {TrueBranch} {(Colon != null ? ':' : '.')} {FalseBranch})";
}