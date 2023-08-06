using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class ParentedExpression : ExpressionAST
{
    public ExpressionAST? Inner;
    public (BracketToken? opening, BracketToken? closing) Parentheses;

    public ParentedExpression((BracketToken?, BracketToken?) parentheses, ExpressionAST? inner)
    {
        Parentheses = parentheses;
        Inner = inner;
    }

    public override string ToString()
    {
        return $"Parens: ({Inner})";
    }
}