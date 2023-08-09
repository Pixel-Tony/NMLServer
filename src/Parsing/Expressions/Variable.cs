using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class Variable : ValueNode
{
    public LiteralToken Value;

    public Variable(ExpressionAST? parent, LiteralToken value) : base(parent)
    {
        Value = value;
    }

    public override string ToString() => $"(Var: {Value})";
}