namespace NMLServer;

internal static class Grammar
{
    private static readonly HashSet<string> _keywords = new()
    {
        "grf", "param", "var", "cargotable", "railtypetable", "roadtypetable", "tramtypetable", "if", "else", "while",
        "item", "property", "graphics", "livery_override", "snowline", "basecost", "template", "spriteset", "switch",
        "spritegroup", "random_switch", "produce", "error", "disable_item", "replace", "replacenew", "font_glyph",
        "deactivate", "town_names", "return", "tilelayout", "spritelayout", "alternative_sprites",
        "base_graphics", "recolour_sprite", "engine_override", "sort"
    };

    static Grammar()
    {
        if (!_keywords.All(k => KeywordTypeByString.ContainsKey(k)))
        {
            throw new Exception();
        }
    }

    public static readonly Dictionary<string, KeywordType> KeywordTypeByString = new()
    {
        ["grf"] = KeywordType.Grf,
        ["param"] = KeywordType.Param,
        ["var"] = KeywordType.Var,
        ["cargotable"] = KeywordType.CargoTable,
        ["railtypetable"] = KeywordType.RailTypeTable,
        ["roadtypetable"] = KeywordType.RoadTypeTable,
        ["tramtypetable"] = KeywordType.TramTypeTable,
        ["if"] = KeywordType.If,
        ["else"] = KeywordType.Else,
        ["while"] = KeywordType.While,
        ["item"] = KeywordType.Item,
        ["property"] = KeywordType.Property,
        ["graphics"] = KeywordType.Graphics,
        ["livery_override"] = KeywordType.LiveryOverride,
        ["snowline"] = KeywordType.SnowLine,
        ["basecost"] = KeywordType.BaseCost,
        ["template"] = KeywordType.Template,
        ["spriteset"] = KeywordType.SpriteSet,
        ["switch"] = KeywordType.Switch,
        ["spritegroup"] = KeywordType.SpriteGroup,
        ["random_switch"] = KeywordType.RandomSwitch,
        ["produce"] = KeywordType.Produce,
        ["error"] = KeywordType.Error,
        ["disable_item"] = KeywordType.DisableItem,
        ["replace"] = KeywordType.Replace,
        ["replacenew"] = KeywordType.ReplaceNew,
        ["font_glyph"] = KeywordType.FontGlyph,
        ["deactivate"] = KeywordType.Deactivate,
        ["town_names"] = KeywordType.TownNames,
        ["return"] = KeywordType.Return,
        ["tilelayout"] = KeywordType.TileLayout,
        ["spritelayout"] = KeywordType.SpriteLayout,
        ["alternative_sprites"] = KeywordType.AlternativeSprites,
        ["base_graphics"] = KeywordType.BaseGraphics,
        ["recolour_sprite"] = KeywordType.RecolourSprite,
        ["engine_override"] = KeywordType.EngineOverride,
        ["sort"] = KeywordType.Sort
    };

    private static readonly HashSet<string> _functionBlockKeywords = new()
    {
        "error", "disable_item", "deactivate", "engine_override", "sort"
    };

    public static readonly HashSet<KeywordType> FunctionBlockKeywords = new(
        from keyword in KeywordTypeByString
        where _functionBlockKeywords.Contains(keyword.Key)
        select keyword.Value
    );

    public static readonly HashSet<KeywordType> ExpressionKeywordTypes = new(
        from keyword in new[] { "param", "var" }
        select KeywordTypeByString[keyword]
    );

    public static readonly HashSet<string> FeatureIdentifiers = new()
    {
        "FEAT_TRAINS", "FEAT_ROADVEHS", "FEAT_SHIPS", "FEAT_AIRCRAFT", "FEAT_STATIONS", "FEAT_CANALS", "FEAT_BRIDGES",
        "FEAT_HOUSES", "FEAT_GLOBALVARS", "FEAT_INDUSTRYTILES", "FEAT_INDUSTRIES", "FEAT_CARGOS", "FEAT_SOUNDEFFECTS",
        "FEAT_AIRPORTS", "FEAT_SIGNALS", "FEAT_OBJECTS", "FEAT_RAILTYPES", "FEAT_AIRPORTTILES", "FEAT_ROADTYPES",
        "FEAT_TRAMTYPES", "FEAT_ROADSTOPS"
    };

    public static readonly HashSet<char> Brackets = new("[]{}()");

    public static readonly HashSet<string> Operators = new()
    {
        ",",
        "?", ":",
        "||",
        "&&",
        "|",
        "^",
        "&",
        "==", "!=", "<=", ">=", "<", ">",
        "<<", ">>", ">>>",
        "+", "-",
        "*", "/", "%",
        "!", "~"
    };

    public const int TernaryOperatorPrecedence = 1;

    private static readonly Dictionary<string, uint> _operatorPrecedence = new()
    {
        [","] = 0,
        ["?"] = TernaryOperatorPrecedence,
        [":"] = TernaryOperatorPrecedence,
        ["||"] = 2,
        ["&&"] = 3,
        ["|"] = 4,
        ["^"] = 5,
        ["&"] = 6,
        ["=="] = 7, ["!="] = 7, ["<="] = 7, [">="] = 7, ["<"] = 7, [">"] = 7,
        ["<<"] = 8, [">>"] = 8, [">>>"] = 8,
        ["+"] = 9, ["-"] = 9,
        ["*"] = 10, ["/"] = 10, ["%"] = 10,
        ["!"] = 11, ["~"] = 11
    };

    private static readonly Dictionary<char, uint> _oneCharOperatorPrecedence = new(
        from
            kvPair in _operatorPrecedence
        where
            kvPair.Key.Length is 1
        select
            new KeyValuePair<char, uint>(kvPair.Key[0], kvPair.Value)
    );

    public static OperatorType GetOperatorType(string value) => value switch
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
        _ => throw new Exception()
    };

    public static OperatorType GetOperatorType(char value) => value switch
    {
        ',' => OperatorType.Comma,
        '?' => OperatorType.QuestionMark,
        ':' => OperatorType.Colon,
        '|' => OperatorType.BinaryOr,
        '^' => OperatorType.BinaryXor,
        '&' => OperatorType.BinaryAnd,
        '<' => OperatorType.Lt,
        '>' => OperatorType.Gt,
        '+' => OperatorType.Plus,
        '-' => OperatorType.Minus,
        '*' => OperatorType.Multiply,
        '/' => OperatorType.Divide,
        '%' => OperatorType.Modula,
        '!' => OperatorType.LogicalNot,
        '~' => OperatorType.BinaryNot,
        _ => throw new Exception()
    };

    public static uint GetOperatorPrecedence(char value) => _oneCharOperatorPrecedence[value];

    public static uint GetOperatorPrecedance(string value) => value switch
    {
        "?" or ":" => TernaryOperatorPrecedence,
        _ => _operatorPrecedence[value]
    };

    public static readonly HashSet<string> FunctionIdentifiers = new()
    {
        "string", "STORE_TEMP", "STORE_PERM", "LOAD_TEMP", "LOAD_PERM"
    };

    public static readonly HashSet<string> Constants = new()
    {
        "SELF", "PARENT"
    };
}