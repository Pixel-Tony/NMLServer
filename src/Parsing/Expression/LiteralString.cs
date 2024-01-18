using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class LiteralString : ValueNode
{
    public LiteralString(ExpressionAST? parent, ValueToken token) : base(parent, token)
    { }
}