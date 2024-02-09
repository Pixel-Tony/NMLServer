namespace NMLServer.Lexing;

internal sealed class UnitToken : Token
{
    private UnitType _unit;
    public int length { get; }

    public UnitToken(int start, UnitType value, int length) : base(start)
    {
        _unit = value;
        this.length = length;
    }
}