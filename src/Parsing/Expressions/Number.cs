using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal sealed class Number : ValueNode
{
    public NumericToken Value;

    public Number(ExpressionAST? parent, NumericToken value) : base(parent)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"Number: ({Value.value})";
    }
}