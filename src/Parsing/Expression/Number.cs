using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal sealed class Number : ValueNode
{
    public Number(ExpressionAST? parent, ValueToken token) : base(parent, token)
    { }
}