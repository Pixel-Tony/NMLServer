using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class LiteralString : BaseValueNode
{
    public LiteralString(ExpressionAST? parent, StringToken token) : base(parent, token)
    { }
}