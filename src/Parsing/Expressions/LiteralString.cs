using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class LiteralString : ValueNode
{
    public LiteralToken Value;

    public LiteralString(LiteralToken value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"Literal string: ({Value.value})";
    }
}