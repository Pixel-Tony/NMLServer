using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class TernaryOperation : ExpressionAST
{
    public ExpressionAST? Condition;
    public ExpressionAST? TrueBranch;
    public ExpressionAST? FalseBranch;

    public TernaryOpToken? QuestionMark;
    public ColonToken? Colon;

    public TernaryOperation(ExpressionAST? condition, ExpressionAST? trueBranch, ExpressionAST? falseBranch,
        TernaryOpToken? questionMark, ColonToken? colon)
    {
        Condition = condition;
        TrueBranch = trueBranch;
        FalseBranch = falseBranch;
        QuestionMark = questionMark;
        Colon = colon;
    }

    public override string ToString() => $"(?:): Cond=({Condition}), Left=({TrueBranch}), Right=({FalseBranch})";
}