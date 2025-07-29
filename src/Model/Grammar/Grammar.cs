using System.Collections.Frozen;
using System.Text;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

using DocumentSymbolKind = EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol.SymbolKind;

namespace NMLServer.Model;

internal static class Grammar
{
    public const int TernaryOperatorPrecedence = 1;

    public static readonly FrozenDictionary<OperatorType, byte> OperatorPrecedences = new Dictionary<OperatorType, byte>
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
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<string, UnitType>.AlternateLookup<StringView> UnitLiterals
        = new Dictionary<string, UnitType>
        {
            ["hp"] = UnitType.HP,
            ["kg"] = UnitType.Kg,
            ["kW"] = UnitType.KW,
            ["hpI"] = UnitType.HpI,
            ["hpM"] = UnitType.HpM,
            ["mph"] = UnitType.MPH,
            ["ton"] = UnitType.Ton,
            ["tons"] = UnitType.Tons
        }.ToFrozenDictionary().GetAlternateLookup<StringView>();

    public static readonly FrozenDictionary<string, (KeywordType type, KeywordKind kind)>.AlternateLookup<StringView>
        Keywords = new Dictionary<string, (KeywordType, KeywordKind)>
        {
            ["if"] = (KeywordType.If, KeywordKind.BlockDefining),
            ["grf"] = (KeywordType.Grf, KeywordKind.BlockDefining),
            ["var"] = (KeywordType.Var, KeywordKind.ExpressionUsable),
            ["else"] = (KeywordType.Else, KeywordKind.BlockDefining),
            ["item"] = (KeywordType.Item, KeywordKind.BlockDefining),
            ["sort"] = (KeywordType.Sort, KeywordKind.BlockDefining),
            ["error"] = (KeywordType.Error, KeywordKind.BlockDefining),
            ["const"] = (KeywordType.Const, KeywordKind.BlockDefining),
            ["param"] = (KeywordType.Param, KeywordKind.ExpressionUsable),
            ["while"] = (KeywordType.While, KeywordKind.BlockDefining),
            ["return"] = (KeywordType.Return, KeywordKind.None),
            ["string"] = (KeywordType.String, KeywordKind.ExpressionUsable),
            ["switch"] = (KeywordType.Switch, KeywordKind.BlockDefining),
            ["produce"] = (KeywordType.Produce, KeywordKind.BlockDefining),
            ["replace"] = (KeywordType.Replace, KeywordKind.BlockDefining),
            ["basecost"] = (KeywordType.BaseCost, KeywordKind.BlockDefining),
            ["graphics"] = (KeywordType.Graphics, KeywordKind.BlockDefining),
            ["property"] = (KeywordType.Property, KeywordKind.BlockDefining),
            ["snowline"] = (KeywordType.SnowLine, KeywordKind.BlockDefining),
            ["template"] = (KeywordType.Template, KeywordKind.BlockDefining),
            ["spriteset"] = (KeywordType.SpriteSet, KeywordKind.BlockDefining),
            ["cargotable"] = (KeywordType.CargoTable, KeywordKind.BlockDefining),
            ["deactivate"] = (KeywordType.Deactivate, KeywordKind.BlockDefining),
            ["font_glyph"] = (KeywordType.FontGlyph, KeywordKind.BlockDefining),
            ["replacenew"] = (KeywordType.ReplaceNew, KeywordKind.BlockDefining),
            ["tilelayout"] = (KeywordType.TileLayout, KeywordKind.BlockDefining),
            ["town_names"] = (KeywordType.TownNames, KeywordKind.BlockDefining),
            ["spritegroup"] = (KeywordType.SpriteGroup, KeywordKind.BlockDefining),
            ["disable_item"] = (KeywordType.DisableItem, KeywordKind.BlockDefining),
            ["spritelayout"] = (KeywordType.SpriteLayout, KeywordKind.BlockDefining),
            ["base_graphics"] = (KeywordType.BaseGraphics, KeywordKind.BlockDefining),
            ["railtypetable"] = (KeywordType.RailTypeTable, KeywordKind.BlockDefining),
            ["random_switch"] = (KeywordType.RandomSwitch, KeywordKind.BlockDefining),
            ["roadtypetable"] = (KeywordType.RoadTypeTable, KeywordKind.BlockDefining),
            ["tramtypetable"] = (KeywordType.TramTypeTable, KeywordKind.BlockDefining),
            ["engine_override"] = (KeywordType.EngineOverride, KeywordKind.BlockDefining),
            ["livery_override"] = (KeywordType.LiveryOverride, KeywordKind.BlockDefining),
            ["recolour_sprite"] = (KeywordType.RecolourSprite, KeywordKind.BlockDefining),
            ["alternative_sprites"] = (KeywordType.AlternativeSprites, KeywordKind.BlockDefining)
        }.ToFrozenDictionary().GetAlternateLookup<StringView>();

    public static readonly FrozenDictionary<string, SymbolKind>.AlternateLookup<StringView> DefinedSymbols;

    static Grammar()
    {
        ReadOnlySpan<(string filename, SymbolKind kind)> entries =
        [
            ("constants.txt", SymbolKind.Constant),
            ("properties.txt", SymbolKind.Property),
            ("variables.txt", SymbolKind.Variable),
            ("functions.txt", SymbolKind.Function),
            ("misc-bits.txt", SymbolKind.Variable | SymbolKind.Writeable),
            ("readable.txt", SymbolKind.Variable),
            ("features.txt", SymbolKind.Feature)
        ];

        Dictionary<string, SymbolKind> allSymbols = [];
        foreach (var (filename, kind) in entries)
        {
            var path = Path.Join(AppContext.BaseDirectory, "grammar", filename);
            var symbols = File.ReadAllText(path, Encoding.UTF8).Split('\n');
            allSymbols.EnsureCapacity(allSymbols.Count + symbols.Length);
            foreach (var symbol in symbols)
            {
                allSymbols[symbol] = kind;
            }
        }
        allSymbols.TrimExcess();
        DefinedSymbols = allSymbols.ToFrozenDictionary().GetAlternateLookup<StringView>();
    }

    public static SymbolKind GetSymbolInfo(StringView needle)
    {
        DefinedSymbols.TryGetValue(needle, out var value);
        return value;
    }

    public static readonly List<string> TokenTypes =
    [
        SemanticTokenTypes.EnumMember, SemanticTokenTypes.Parameter, SemanticTokenTypes.Number, SemanticTokenTypes.Type,
        SemanticTokenTypes.String, SemanticTokenTypes.Keyword, SemanticTokenTypes.Variable, SemanticTokenTypes.Comment,
        SemanticTokenTypes.Operator, SemanticTokenTypes.Function, SemanticTokenTypes.Property
    ];

    public static readonly List<string> TokenModifiers = [];

    public static CompletionItemKind GetCompletionItemKind(SymbolKind kind) => (kind & SymbolKind.KindMask) switch
    {
        SymbolKind.Feature => CompletionItemKind.Class,
        SymbolKind.Function => CompletionItemKind.Function,
        SymbolKind.Variable => CompletionItemKind.Variable,
        SymbolKind.Parameter => CompletionItemKind.Variable,
        SymbolKind.Constant => CompletionItemKind.Constant,
        SymbolKind.Property => CompletionItemKind.Property,
        _ => 0
    };

    public static DocumentSymbolKind GetDocumentSymbolKind(SymbolKind kind) => (kind & SymbolKind.KindMask) switch
    {
        SymbolKind.Feature => DocumentSymbolKind.Class,
        SymbolKind.Function => DocumentSymbolKind.Function,
        SymbolKind.Variable => DocumentSymbolKind.Variable,
        SymbolKind.Parameter => DocumentSymbolKind.Variable,
        SymbolKind.Constant => DocumentSymbolKind.Constant,
        SymbolKind.Property => DocumentSymbolKind.Property,
        _ => 0
    };

    public static string? GetSemanticTokenType(SymbolKind kind) => (kind & SymbolKind.KindMask) switch
    {
        SymbolKind.Feature => SemanticTokenTypes.Type,
        SymbolKind.Function => SemanticTokenTypes.Function,
        SymbolKind.Variable => SemanticTokenTypes.Variable,
        SymbolKind.Parameter => SemanticTokenTypes.Parameter,
        SymbolKind.Constant => SemanticTokenTypes.EnumMember,
        SymbolKind.Property => SemanticTokenTypes.Property,
        _ => null
    };
}