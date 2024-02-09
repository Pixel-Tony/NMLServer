using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class BinaryOperation : ExpressionAST
{
    private readonly BinaryOpToken _operation;
    private ExpressionAST? _left;
    public ExpressionAST? Right;

    public uint precedence => _operation.Precedence;

    public BinaryOperation(ExpressionAST? parent, BinaryOpToken op) : base(parent)
    {
        _operation = op;
    }

    public BinaryOperation(ExpressionAST? parent, ExpressionAST? lhs, BinaryOpToken op) : this(parent, op)
    {
        _left = lhs;
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