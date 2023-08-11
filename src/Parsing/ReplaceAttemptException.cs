namespace NMLServer.Parsing.Expression;

internal class ReplaceAttemptException : Exception
{
    public ReplaceAttemptException(ExpressionAST node)
        : base($"Cannot replace child: {node.GetType()} is a bottom-level node")
    { }
}