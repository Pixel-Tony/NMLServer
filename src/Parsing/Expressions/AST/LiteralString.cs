using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class LiteralString : ValueNode<StringToken>
{
    public LiteralString(ExpressionAST? parent, StringToken str) : base(parent, str)
    { }

    public override string ToString() => Value?.value ?? ".";
}