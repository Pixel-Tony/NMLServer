using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class BinaryOperation : ExpressionAST
{
    public ExpressionAST? Left;
    public BinaryOpToken Operation;
    public ExpressionAST? Right;

    public BinaryOperation(ExpressionAST? left, BinaryOpToken operation, ExpressionAST? right)
    {
        Left = left;
        Operation = operation;
        Right = right;
    }

    public override string ToString() =>
        $"Bin ({Operation.Operation}), Left={{{Left}}}, Right={{{Right}}}";
}