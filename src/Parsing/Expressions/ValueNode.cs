namespace NMLServer.Parsing.Expression;

internal abstract class ValueNode : ExpressionAST
{
    protected ValueNode(ExpressionAST? parent) : base(parent)
    { }

    public sealed override ExpressionAST Replace(ExpressionAST target, ExpressionAST value)
    {
        return base.Replace(target, value);
    }
}