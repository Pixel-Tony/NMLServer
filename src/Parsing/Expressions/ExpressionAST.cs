namespace NMLServer.Parsing.Expression;

internal abstract class ExpressionAST
{
    public ExpressionAST? Parent;

    protected ExpressionAST(ExpressionAST? parent) => Parent = parent;

    public virtual ExpressionAST Replace(ExpressionAST target, ExpressionAST value)
        => throw new ReplaceAttemptException(this);

    public abstract override string ToString();
}