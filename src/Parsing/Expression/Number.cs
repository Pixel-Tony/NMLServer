using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal sealed class Number : ValueNode<BaseValueToken>
{
    public Number(ExpressionAST? parent, BaseValueToken token) : base(parent, token)
    { }
}