using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal abstract class ValueNode<T> : ExpressionAST where T : Token
{
    protected readonly T? Value;

    protected ValueNode(ExpressionAST? parent, T? value) : base(parent)
    {
        Value = value;
    }

    public sealed override ExpressionAST Replace(ExpressionAST target, ExpressionAST value)
    {
        return base.Replace(target, value);
    }
}