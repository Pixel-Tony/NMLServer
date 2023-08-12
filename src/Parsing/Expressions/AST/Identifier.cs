using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class Identifier : ValueNode<BaseValueToken>
{
    public Identifier(ExpressionAST? parent, BaseValueToken token) : base(parent, token)
    { }
}