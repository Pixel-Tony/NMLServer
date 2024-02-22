using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal abstract class BaseValueNode(ExpressionAST? parent) : ExpressionAST(parent)
{
    public abstract BaseValueToken token { get; }
}

internal abstract class BaseValueNode<T>(ExpressionAST? parent, T token) : BaseValueNode(parent)
    where T : BaseValueToken
{
    protected readonly T Token = token;

    public sealed override BaseValueToken token => Token;

    public sealed override int start => Token.start;
    public sealed override int end => Token.start + Token.length;
}