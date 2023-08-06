using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal sealed class Number : ValueNode
{
    public readonly NumericToken Value;

    public Number(NumericToken value) => Value = value;
    public override string ToString()
    {
        return $"Number: ({Value.value})";
    }
}