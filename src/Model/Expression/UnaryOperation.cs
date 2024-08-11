using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class UnaryOperation(ExpressionAST? parent, UnaryOpToken operation) : ExpressionAST(parent)
{
    public ExpressionAST? Expression;

    public override int start => operation.start;

    public override int end => Expression?.end ?? operation.end;

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        Expression = value;
    }
}