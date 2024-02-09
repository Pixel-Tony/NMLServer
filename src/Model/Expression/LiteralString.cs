using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class LiteralString : BaseValueNode
{
    public LiteralString(ExpressionAST? parent, StringToken token) : base(parent, token)
    { }
}