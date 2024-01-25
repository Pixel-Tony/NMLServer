namespace NMLServer.Lexing.Tokens;

internal sealed class BinaryOpToken : MulticharToken
{
    public readonly uint Precedence;
    public readonly OperatorType Type;

    public BinaryOpToken(int start, int end, string value) : base(start, end)
    {
        Precedence = Grammar.GetOperatorPrecedance(value);
        Type = Grammar.GetOperatorType(value);
    }

    public BinaryOpToken(int start, int end, char value) : base(start, end)
    {
        Precedence = Grammar.GetOperatorPrecedence(value);
        Type = Grammar.GetOperatorType(value);
    }
}