using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal sealed class Number : ValueNode<NumericToken>
{
    public Number(ExpressionAST? parent, NumericToken value) : base(parent, value)
    { }

    public override string ToString() => Value?.value ?? "null";
}