namespace NMLServer.Lexing;

internal sealed class UnitToken(int start, UnitType value) : Token(start)
{
    public override int length { get; } = (byte)value >> 4;

    public override int end => start + length;
}