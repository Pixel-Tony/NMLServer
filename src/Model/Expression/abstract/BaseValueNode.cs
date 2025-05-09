using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal abstract class BaseValueNode(ExpressionAST? parent) : ExpressionAST(parent)
{
    public abstract BaseValueToken token { get; }
}

internal abstract class BaseValueNode<T>(ExpressionAST? parent, T value) : BaseValueNode(parent)
    where T : BaseValueToken
{
    public sealed override T token => value;

    public sealed override int start => value.start;
    public sealed override int end => value.end;
}