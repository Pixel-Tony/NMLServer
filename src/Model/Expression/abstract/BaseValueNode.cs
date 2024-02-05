using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal abstract class BaseValueNode : ExpressionAST
{
    public readonly BaseValueToken Token;

    protected BaseValueNode(ExpressionAST? parent, BaseValueToken recordingToken) : base(parent)
    {
        Token = recordingToken;
    }
}