using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class LiteralString : ValueNode<BaseValueToken>
{
    public LiteralString(ExpressionAST? parent, BaseValueToken token) : base(parent, token)
    { }
}