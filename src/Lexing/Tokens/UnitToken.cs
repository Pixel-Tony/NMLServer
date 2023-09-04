namespace NMLServer.Lexing.Tokens;

internal class UnitToken : Token
{
    private UnitType _unitType;

    public UnitToken(int start, string value) : base(start)
    {
        _unitType = Grammar.Units[value];
    }

    public UnitToken(int start, UnitType type) : base(start)
    {
        _unitType = type;
    }
}