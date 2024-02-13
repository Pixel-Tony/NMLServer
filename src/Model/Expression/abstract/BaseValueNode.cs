using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal abstract class BaseValueNode : ExpressionAST
{
    public abstract BaseValueToken token { get; }

    protected BaseValueNode(ExpressionAST? parent) : base(parent)
    { }
}

internal abstract class BaseValueNode<T> : BaseValueNode where T : BaseValueToken
{
    protected readonly T Token;

    public sealed override BaseValueToken token => Token;

    public sealed override int start => Token.Start;
    public sealed override int end => Token.Start + Token.Length;

    protected BaseValueNode(ExpressionAST? parent, T token) : base(parent)
    {
        Token = token;
    }
}