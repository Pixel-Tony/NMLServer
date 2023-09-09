namespace NMLServer.Lexing.Tokens;

internal class BinaryOpToken : BaseMulticharToken, IHasPrecedence
{
    public int precedence { get; }
    public readonly OperatorType Type;

    public BinaryOpToken(int start, int end, string value) : base(start, end)
    {
        precedence = Grammar.GetOperatorPrecedance(value);
        Type = Grammar.GetOperatorType(value);
    }

    public BinaryOpToken(int start, int end, char value) : base(start, end)
    {
        precedence = Grammar.GetOperatorPrecedence(value);
        Type = Grammar.GetOperatorType(value);
    }

    public static bool operator >(BinaryOpToken left, IHasPrecedence right) => left.precedence > right.precedence;
    public static bool operator <(BinaryOpToken left, IHasPrecedence right) => left.precedence < right.precedence;
}