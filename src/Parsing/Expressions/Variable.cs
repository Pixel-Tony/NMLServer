using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class Variable : ValueNode
{
    public LiteralToken Value;

    public Variable(LiteralToken value)
    {
        Value = value;
    }

    public override string ToString() => $"Variable: {Value.value}";
}