using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class BinaryOperation : ExpressionAST
{
    private readonly BinaryOpToken _operation;
    private ExpressionAST? _left;
    public ExpressionAST? Right;

    public readonly uint Precedence;

    public BinaryOperation(ExpressionAST? parent, BinaryOpToken binaryOpToken) : base(parent)
    {
        _operation = binaryOpToken;
        Precedence = binaryOpToken.Precedence;
    }

    public BinaryOperation(ExpressionAST? parent, ExpressionAST? left, BinaryOpToken op) : this(parent, op)
    {
        _left = left;
    }

    protected override void Replace(ExpressionAST target, ExpressionAST value)
    {
        if (_left == target)
        {
            _left = value;
            return;
        }
        Right = value;
    }
}