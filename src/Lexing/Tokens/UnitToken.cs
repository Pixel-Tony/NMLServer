namespace NMLServer.Lexing.Tokens;

internal sealed class UnitToken : Token
{
    private UnitType _unit;
    public ushort length { get; }

    public UnitToken(int start, UnitType value, int length) : base(start)
    {
        _unit = value;
        this.length = (ushort)length;
    }
}