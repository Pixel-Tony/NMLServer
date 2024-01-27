using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal abstract class ValueNode : ExpressionAST
{
    public readonly ValueToken Token;

    protected ValueNode(ExpressionAST? parent, ValueToken recordingToken) : base(parent)
    {
        Token = recordingToken;
    }
}