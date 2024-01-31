namespace NMLServer;

internal static class Grammar
{
    public static readonly HashSet<string> FeatureIdentifiers = new()
    {
        "FEAT_TRAINS", "FEAT_ROADVEHS", "FEAT_SHIPS", "FEAT_AIRCRAFT", "FEAT_STATIONS", "FEAT_CANALS", "FEAT_BRIDGES",
        "FEAT_HOUSES", "FEAT_GLOBALVARS", "FEAT_INDUSTRYTILES", "FEAT_INDUSTRIES", "FEAT_CARGOS", "FEAT_SOUNDEFFECTS",
        "FEAT_AIRPORTS", "FEAT_SIGNALS", "FEAT_OBJECTS", "FEAT_RAILTYPES", "FEAT_AIRPORTTILES", "FEAT_ROADTYPES",
        "FEAT_TRAMTYPES", "FEAT_ROADSTOPS"
    };

    public const int TernaryOperatorPrecedence = 1;

    public static readonly Dictionary<OperatorType, byte> OperatorPrecedences = new()
    {
        [OperatorType.Comma] = 0,
        [OperatorType.QuestionMark] = TernaryOperatorPrecedence,
        [OperatorType.Colon] = TernaryOperatorPrecedence,
        [OperatorType.LogicalOr] = 2,
        [OperatorType.LogicalAnd] = 3,
        [OperatorType.BinaryOr] = 4,
        [OperatorType.BinaryXor] = 5,
        [OperatorType.BinaryAnd] = 6,
        [OperatorType.Eq] = 7,
        [OperatorType.Ne] = 7,
        [OperatorType.Le] = 7,
        [OperatorType.Ge] = 7,
        [OperatorType.Lt] = 7,
        [OperatorType.Gt] = 7,
        [OperatorType.ShiftLeft] = 8,
        [OperatorType.ShiftRight] = 8,
        [OperatorType.ShiftRightFunky] = 8,
        [OperatorType.Plus] = 9,
        [OperatorType.Minus] = 9,
        [OperatorType.Multiply] = 10,
        [OperatorType.Divide] = 10,
        [OperatorType.Modula] = 10,
        [OperatorType.LogicalNot] = 11,
        [OperatorType.BinaryNot] = 11
    };

    public static OperatorType GetOperatorType(ReadOnlySpan<char> needle)
        => needle switch
        {
            "," => OperatorType.Comma,
            "?" => OperatorType.QuestionMark,
            ":" => OperatorType.Colon,
            "||" => OperatorType.LogicalOr,
            "&&" => OperatorType.LogicalAnd,
            "|" => OperatorType.BinaryOr,
            "^" => OperatorType.BinaryXor,
            "&" => OperatorType.BinaryAnd,
            "==" => OperatorType.Eq,
            "!=" => OperatorType.Ne,
            "<=" => OperatorType.Le,
            ">=" => OperatorType.Ge,
            "<" => OperatorType.Lt,
            ">" => OperatorType.Gt,
            "<<" => OperatorType.ShiftLeft,
            ">>" => OperatorType.ShiftRight,
            ">>>" => OperatorType.ShiftRightFunky,
            "+" => OperatorType.Plus,
            "-" => OperatorType.Minus,
            "*" => OperatorType.Multiply,
            "/" => OperatorType.Divide,
            "%" => OperatorType.Modula,
            "!" => OperatorType.LogicalNot,
            "~" => OperatorType.BinaryNot,
            _ => OperatorType.None
        };

    public static OperatorType GetOperatorType(char needle) => GetOperatorType(stackalloc char[] { needle });

    public static readonly HashSet<string> FunctionIdentifiers = new()
    {
        "string", "STORE_TEMP", "STORE_PERM", "LOAD_TEMP", "LOAD_PERM"
    };

    public static readonly HashSet<string> Constants = new()
    {
        "SELF", "PARENT"
    };
}