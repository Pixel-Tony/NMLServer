namespace NMLServer.Lexing;

internal sealed class UnitToken : Token
{
    private UnitType _unit;
    public int length { get; }

    public UnitToken(int start, UnitType value) : base(start)
    {
        _unit = value;
        length = (byte)value >> 4;
    }
}