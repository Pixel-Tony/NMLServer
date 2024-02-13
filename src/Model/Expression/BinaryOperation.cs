using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class BinaryOperation : ExpressionAST
{
    private ExpressionAST? _left;
    private readonly BinaryOpToken _operation;
    public ExpressionAST? Right;

    public override int start => _left?.start ?? _operation.Start;
    public override int end => Right?.end ?? _operation.Start + _operation.Length;

    public uint precedence => _operation.Precedence;

    public BinaryOperation(ExpressionAST? parent, BinaryOpToken op) : base(parent)
    {
        _operation = op;
    }

    public BinaryOperation(ExpressionAST? parent, ExpressionAST? lhs, BinaryOpToken op) : this(parent, op)
    {
        _left = lhs;
    }

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        if (_left == target)
        {
            _left = value;
            return;
        }
        Right = value;
    }
}