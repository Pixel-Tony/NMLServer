using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal abstract class BaseValueNode : ExpressionAST
{
    public readonly BaseValueToken Token;

    protected BaseValueNode(ExpressionAST? parent, BaseValueToken token) : base(parent)
    {
        Token = token;
    }
}