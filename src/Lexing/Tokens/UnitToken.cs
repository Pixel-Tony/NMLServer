namespace NMLServer.Lexing.Tokens;

internal class UnitToken : Token
{
    private UnitType _unitType;

    public UnitToken(int start, string value) : base(start)
    {
        _unitType = _mapping[value];
    }

    public UnitToken(int start, UnitType type) : base(start)
    {
        _unitType = type;
    }

    public enum UnitType
    {
        MPH, 
        KMPH,
        MPS,
        HP,
        KW,
        HpI,
        HpM,
        Tons,
        Ton,
        Kg
    }

    public static bool IsUnit(string value) => _mapping.ContainsKey(value);

    private static readonly Dictionary<string, UnitType> _mapping = new()
    {
        ["km/h"] = UnitType.KMPH,
        ["m/s"] = UnitType.MPS,
        ["mph"] = UnitType.MPH,
        ["hp"] = UnitType.HP,
        ["kW"] = UnitType.KW,
        ["hpI"] = UnitType.HpI,
        ["hpM"] = UnitType.HpM,
        ["tons"] = UnitType.Tons,
        ["ton"] = UnitType.Ton,
        ["kg"] = UnitType.Kg
    };
}