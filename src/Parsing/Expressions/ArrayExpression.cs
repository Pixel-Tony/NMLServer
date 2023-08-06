using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class ArrayExpression : ExpressionAST
{
    public BracketToken? OpeningBracket;
    public ExpressionAST? Expression;
    public BracketToken? ClosingBracket;

    public ArrayExpression(BracketToken? openingBracket, ExpressionAST? expression, BracketToken? closingBracket)
    {
        OpeningBracket = openingBracket;
        Expression = expression;
        ClosingBracket = closingBracket;
    }

    public override string ToString() => $"Array [{Expression}]";
}