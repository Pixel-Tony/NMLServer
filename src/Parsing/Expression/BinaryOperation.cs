using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class BinaryOperation : ExpressionAST, IHasPrecedence
{
    private readonly BinaryOpToken _operation;
    private ExpressionAST? _left;
    public ExpressionAST? Right;

    public int precedence => _operation.precedence;

    public static bool operator >(BinaryOperation left, IHasPrecedence right) => left.precedence > right.precedence;
    public static bool operator <(BinaryOperation left, IHasPrecedence right) => left.precedence < right.precedence;

    public BinaryOperation(ExpressionAST? parent, BinaryOpToken binaryOpToken) : base(parent) =>
        _operation = binaryOpToken;

    public BinaryOperation(ExpressionAST? parent, ExpressionAST? left, BinaryOpToken op) : this(parent, op) =>
        _left = left;

    public override void Replace(ExpressionAST target, ExpressionAST value)
    {
        if (_left == target)
        {
            _left = value;
            return;
        }
        Right = value;
    }
}