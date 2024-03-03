namespace NMLServer.Lexing;

internal sealed class BinaryOpToken(int start, int end, OperatorType type) : BaseMulticharToken(start, end)
{
    public readonly byte Precedence = Grammar.OperatorPrecedences[type];
    public readonly OperatorType Type = type;
}