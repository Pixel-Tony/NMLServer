using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class UnaryOperation : ExpressionAST
{
    private readonly UnaryOpToken _operation;
    public ExpressionAST? Expression;

    public UnaryOperation(ExpressionAST? parent, UnaryOpToken operation) : base(parent)
    {
        _operation = operation;
    }

    protected override void Replace(ExpressionAST target, ExpressionAST value)
    {
        Expression = value;
    }
}