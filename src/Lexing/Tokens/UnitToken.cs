namespace NMLServer.Lexing.Tokens;

internal sealed class UnitToken : Token
{
    private UnitType _unit;
    public byte length { get; }

    public UnitToken(int start, UnitType value, byte length) : base(start)
    {
        _unit = value;
        this.length = length;
    }

    public static bool IsLiteralUnit(ReadOnlySpan<char> target, out (UnitType type, byte length) result)
    {
        switch (target)
        {
            case "mph":
                result = (UnitType.MPH, 3);
                return true;

            case "hp":
                result = (UnitType.HP, 2);
                return true;

            case "kW":
                result = (UnitType.KW, 2);
                return true;

            case "hpI":
                result = (UnitType.HpI, 3);
                return true;

            case "hpM":
                result = (UnitType.HpM, 3);
                return true;

            case "tons":
                result = (UnitType.Tons, 4);
                return true;

            case "ton":
                result = (UnitType.Ton, 3);
                return true;

            case "kg":
                result = (UnitType.Kg, 2);
                return true;

            default:
                result = (UnitType.Kg, 0);
                return false;
        }
    }
}