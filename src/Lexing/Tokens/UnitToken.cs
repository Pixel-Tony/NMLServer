namespace NMLServer.Lexing.Tokens;

internal class UnitToken : Token
{
    private UnitType _unit;

    public UnitToken(int start, UnitType value) : base(start)
    {
        _unit = value;
    }
}