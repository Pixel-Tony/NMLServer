namespace NMLServer.Parsing;

internal static class Grammar
{
    public static readonly HashSet<string> Keywords = new()
    {
        "grf", "param", "var", "cargotable", "railtypetable", "roadtypetable", "tramtypetable", "if", "else", "while", 
        "item", "property", "graphics", "livery_override", "snowline", "basecost", "template", "spriteset", "switch", 
        "spritegroup", "random_switch", "produce", "error", "disable_item", "replace", "replacenew", "font_glyph", 
        "deactivate", "town_names", "string", "return", "exit", "tilelayout", "spritelayout", "alternative_sprites",
        "base_graphics", "recolour_sprite", "engine_override", "sort"
    };

    // keyword => does it use square braces for argument list
    public static readonly Dictionary<string, bool> ExpressionKeywords = new()
    {
        ["param"] = true, 
        ["var"] = true,
        ["string"] = false,
        ["sort"] = false,
    };
    
    public static readonly HashSet<string> NotExpressionKeywords;

    public static readonly HashSet<string> Features = new()
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

    public static readonly Dictionary<string, int> OperatorPrecedence = new()
    {
        [","] = 0,
        ["?"] = 1, [":"] = 1,
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
    
    static Grammar()
    {
        Keywords.UnionWith(Features);
        NotExpressionKeywords = new HashSet<string>(Keywords);
        NotExpressionKeywords.ExceptWith(ExpressionKeywords.Keys);
    }
}