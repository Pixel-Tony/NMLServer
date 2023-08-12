using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class BinaryOperation : ExpressionAST, IHasPrecedence
{
    private ExpressionAST? _left;
    private readonly BinaryOpToken _operation;
    public ExpressionAST? Right;

    public int precedence => _operation.precedence;

    public static bool operator >(BinaryOperation left, IHasPrecedence right) => left.precedence > right.precedence;
    public static bool operator <(BinaryOperation left, IHasPrecedence right) => left.precedence < right.precedence;

    public override string ToString() =>
        $"({_left?.ToString() ?? "."} {_operation.Operation} {Right?.ToString() ?? "."})";

    public BinaryOperation(ExpressionAST? parent, BinaryOpToken binaryOpToken) : base(parent) =>
        _operation = binaryOpToken;

    public BinaryOperation(ExpressionAST? parent, ExpressionAST? left, BinaryOpToken op) : this(parent, op) =>
        _left = left;

    public override ExpressionAST Replace(ExpressionAST target, ExpressionAST value)
    {
        if (_left == target)
        {
            _left = value;
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
}