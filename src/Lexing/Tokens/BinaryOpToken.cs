namespace NMLServer.Lexing.Tokens;

internal class BinaryOpToken : BaseMulticharToken, IHasPrecedence
{
    public int precedence { get; }

    public BinaryOpToken(int start, int end, string value) : base(start, end)
    {
        precedence = Grammar.OperatorPrecedence[value];
    }

    public BinaryOpToken(int start, int end, char value) : base(start, end)
    {
        precedence = Grammar.GetOperatorPrecedence(value);
    }

    public static bool operator >(BinaryOpToken left, IHasPrecedence right) => left.precedence > right.precedence;
    public static bool operator <(BinaryOpToken left, IHasPrecedence right) => left.precedence < right.precedence;
}