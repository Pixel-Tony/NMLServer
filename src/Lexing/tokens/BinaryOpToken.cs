namespace NMLServer.Lexing;

internal sealed class BinaryOpToken : BaseMulticharToken
{
    public readonly byte Precedence;
    public readonly OperatorType Type;

    public BinaryOpToken(int start, int end, OperatorType type) : base(start, end)
    {
        Precedence = Grammar.OperatorPrecedences[type];
        Type = type;
    }
}