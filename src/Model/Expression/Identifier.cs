using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class Identifier : BaseValueNode
{
    public Identifier(ExpressionAST? parent, IdentifierToken token) : base(parent, token)
    { }
}