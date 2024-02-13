using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class UnaryOperation : ExpressionAST
{
    private readonly UnaryOpToken _operation;
    public ExpressionAST? Expression;

    public override int start => _operation.Start;

    public override int end => Expression?.end ?? _operation.Start + 1;

    public UnaryOperation(ExpressionAST? parent, UnaryOpToken operation) : base(parent)
    {
        _operation = operation;
    }

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        Expression = value;
    }
}