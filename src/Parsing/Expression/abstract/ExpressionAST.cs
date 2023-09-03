namespace NMLServer.Parsing.Expression;

internal abstract class ExpressionAST
{
    public ExpressionAST? Parent;

    protected ExpressionAST(ExpressionAST? parent) => Parent = parent;

    public abstract void Replace(ExpressionAST target, ExpressionAST value);

    protected void FailReplacement()
    {
        throw new ReplaceAttemptException(this);
    }
}