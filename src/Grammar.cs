namespace NMLServer;

internal static class Grammar
{
    private static readonly HashSet<string> _keywords = new()
    {
        "grf", "param", "var", "cargotable", "railtypetable", "roadtypetable", "tramtypetable", "if", "else", "while",
        "item", "property", "graphics", "livery_override", "snowline", "basecost", "template", "spriteset", "switch",
        "spritegroup", "random_switch", "produce", "error", "disable_item", "replace", "replacenew", "font_glyph",
        "deactivate", "town_names", "return", "exit", "tilelayout", "spritelayout", "alternative_sprites",
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
        ["exit"] = KeywordType.Exit,
        ["tilelayout"] = KeywordType.TileLayout,
        ["spritelayout"] = KeywordType.SpriteLayout,
        ["alternative_sprites"] = KeywordType.AlternativeSprites,
        ["base_graphics"] = KeywordType.BaseGraphics,
        ["recolour_sprite"] = KeywordType.RecolourSprite,
        ["engine_override"] = KeywordType.EngineOverride,
        ["sort"] = KeywordType.Sort
    };

    private static readonly HashSet<string> _blockKeywords = new()
    {
        "grf", "param", "cargotable", "railtypetable", "roadtypetable", "tramtypetable", "if", "else", "while", "item",
        "property", "graphics", "livery_override", "snowline", "basecost", "template", "spriteset", "switch",
        "spritegroup", "random_switch", "replace", "replacenew", "font_glyph", "town_names", "tilelayout",
        "spritelayout", "alternative_sprites", "base_graphics", "recolour_sprite",
    };

    public static readonly HashSet<KeywordType> BlockKeywordTypes
        = KeywordTypeByString
            .Where(p => _blockKeywords.Contains(p.Key))
            .Select(p => p.Value)
            .ToHashSet();

    private static readonly HashSet<string> _expressionKeywords = new()
    {
        "param",
        "var"
    };

    public static readonly HashSet<KeywordType> ExpressionKeywordTypes =
        _expressionKeywords.Select(p => KeywordTypeByString[p]).ToHashSet();

    private static readonly HashSet<string> _featureIdentifiers = new()
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

    public static readonly Dictionary<string, int> OperatorPrecedence = new()
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

    public static readonly Dictionary<string, UnitType> Units = new()
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

    private static readonly Dictionary<char, int> _oneCharOperatorPrecedence = OperatorPrecedence
        .Where(p => p.Key.Length == 1)
        .ToDictionary(kv => kv.Key[0], kv => kv.Value);

    public static int GetOperatorPrecedence(char value) => _oneCharOperatorPrecedence[value];
}