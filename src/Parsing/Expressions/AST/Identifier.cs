using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class Identifier : ValueNode<BaseRecordingToken>
{
    public Identifier(ExpressionAST? parent, BaseRecordingToken value) : base(parent, value)
    { }

    public override string ToString() => Value?.value ?? "null";
}