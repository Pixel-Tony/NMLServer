using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal abstract class ValueToken : MulticharToken
{
    protected ValueToken(int start, int end) : base(start, end)
    { }
}

internal static class ValueNodeFactory
{
    public static ValueNode Make(ExpressionAST? parent, ValueToken token)
    {
        return token switch
        {
            IdentifierToken tok => new Identifier(parent, tok),
            NumericToken tok => new Number(parent, tok),
            StringToken tok => new LiteralString(parent, tok),
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };
    }
}