using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class BinaryOperation : ExpressionAST
{
    public ExpressionAST? Left;
    public BinaryOpToken Operation;
    public ExpressionAST? Right;

    public BinaryOperation(ExpressionAST? parent, BinaryOpToken binaryOpToken) : base(parent)
    {
        Operation = binaryOpToken;
    }

    public override ExpressionAST Replace(ExpressionAST target, ExpressionAST value)
    {
        if (Left == target)
        {
            Left = value;
        }
        else if (Right != target)
        {
            throw new Exception();
        }
        else
        {
            Right = value;
        }
        
        return value;
    }

    public override string ToString() => $"({Left} {Operation?.Operation} {Right})";
}