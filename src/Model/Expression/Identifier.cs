using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class Identifier : BaseValueNode
{
    public Identifier(ExpressionAST? parent, IdentifierToken token) : base(parent, token)
    { }
}