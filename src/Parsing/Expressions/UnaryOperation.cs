using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class UnaryOperation : ExpressionAST
{
    public bool IsLogical;
    public ExpressionAST? Expression;

    public UnaryOperation(UnaryOpToken token, ExpressionAST? expression)
    {
        IsLogical = token.IsLogical;
        Expression = expression;
    }

    public override string ToString()
    {
        return $"Unary ({(IsLogical ? '!' : '~')}): Expr=({Expression})";
    }
}