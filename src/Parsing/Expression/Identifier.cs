using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class Identifier : ValueNode
{
    public Identifier(ExpressionAST? parent, ValueToken token) : base(parent, token)
    { }
}