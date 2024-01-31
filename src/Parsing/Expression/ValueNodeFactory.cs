using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal static class ValueNodeFactory
{
    public static BaseValueNode Make(ExpressionAST? parent, BaseValueToken token)
    {
        return token switch
        {
            IdentifierToken tok => new Identifier(parent, tok),
            NumericToken tok => new Number(parent, tok),
            StringToken tok => new LiteralString(parent, tok),
            _ => throw new ArgumentOutOfRangeException(nameof(token), "Unexpected token tyep")
        };
    }
}