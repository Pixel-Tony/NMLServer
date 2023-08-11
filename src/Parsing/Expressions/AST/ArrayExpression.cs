using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class ArrayExpression : ExpressionAST
{
    public BracketToken? OpeningBracket;
    public ExpressionAST? Expression;
    public BracketToken? ClosingBracket;

    public ArrayExpression(ExpressionAST? parent, BracketToken? openingBracket) : base(parent)
    {
        OpeningBracket = openingBracket;
    }

    public override string ToString() => $"([{Expression}])";
}