namespace NMLServer.Lexing.Tokens;

internal class UnitToken : Token
{
    private UnitType _unit;

    public UnitToken(int start, string value) : base(start)
    {
        _unit = Grammar.Units[value];
    }

    public UnitToken(int start, UnitType type) : base(start)
    {
        _unit = type;
    }
}