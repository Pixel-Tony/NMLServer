using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class BinaryOperation(ExpressionAST? parent, BinaryOpToken op) : ExpressionAST(parent)
{
    private ExpressionAST? _left;
    public ExpressionAST? Right;

    public override int start => _left?.start ?? op.start;
    public override int end => Right?.end ?? op.start + op.length;

    public uint precedence => op.Precedence;

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