using System.Collections.Frozen;
using System.Text;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

using DocumentSymbolKind = EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol.SymbolKind;
using SemanticTokenTypes = EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken.SemanticTokenTypes;

namespace NMLServer.Model.Grammar;

internal static class NML
{
    public static readonly FrozenDictionary<string, Keyword>.AlternateLookup<StringView>
        Keywords = new Dictionary<string, Keyword>
        {
            ["if"] = Keyword.If,
            ["grf"] = Keyword.Grf,
            ["var"] = Keyword.Var,
            ["else"] = Keyword.Else,
            ["item"] = Keyword.Item,
            ["sort"] = Keyword.Sort,
            ["error"] = Keyword.Error,
            ["const"] = Keyword.Const,
            ["param"] = Keyword.Param,
            ["while"] = Keyword.While,
            ["return"] = Keyword.Return,
            ["string"] = Keyword.String,
            ["switch"] = Keyword.Switch,
            ["produce"] = Keyword.Produce,
            ["replace"] = Keyword.Replace,
            ["basecost"] = Keyword.BaseCost,
            ["graphics"] = Keyword.Graphics,
            ["property"] = Keyword.Property,
            ["snowline"] = Keyword.SnowLine,
            ["template"] = Keyword.Template,
            ["spriteset"] = Keyword.SpriteSet,
            ["cargotable"] = Keyword.CargoTable,
            ["deactivate"] = Keyword.Deactivate,
            ["font_glyph"] = Keyword.FontGlyph,
            ["replacenew"] = Keyword.ReplaceNew,
            ["tilelayout"] = Keyword.TileLayout,
            ["town_names"] = Keyword.TownNames,
            ["spritegroup"] = Keyword.SpriteGroup,
            ["disable_item"] = Keyword.DisableItem,
            ["spritelayout"] = Keyword.SpriteLayout,
            ["base_graphics"] = Keyword.BaseGraphics,
            ["railtypetable"] = Keyword.RailTypeTable,
            ["random_switch"] = Keyword.RandomSwitch,
            ["roadtypetable"] = Keyword.RoadTypeTable,
            ["tramtypetable"] = Keyword.TramTypeTable,
            ["engine_override"] = Keyword.EngineOverride,
            ["livery_override"] = Keyword.LiveryOverride,
            ["recolour_sprite"] = Keyword.RecolourSprite,
            ["alternative_sprites"] = Keyword.AlternativeSprites
        }.ToFrozenDictionary().GetAlternateLookup<StringView>();

    public enum UnitType
    {
        None = 0,

        Kg = 0x21,
        HP = 0x22,
        KW = 0x23,

        MPH = 0x31,
        MPS = 0x32,
        Ton = 0x33,
        HpI = 0x34,
        HpM = 0x35,

        KMPH = 0x41,
        Tons = 0x42,
        Snow = 0x53
    }

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

    public static readonly FrozenDictionary<string, SymbolKind>.AlternateLookup<StringView> DefinedSymbols;

    static NML()
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
                allSymbols[symbol] = kind;
        }
        allSymbols.TrimExcess();
        DefinedSymbols = allSymbols.ToFrozenDictionary().GetAlternateLookup<StringView>();
    }

    public static SymbolKind GetSymbolKind(StringView needle)
        => DefinedSymbols.TryGetValue(needle, out var value) ? value : SymbolKind.None;

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

    public static string? GetSemanticTokenType(SymbolKind kind) => (kind & SymbolKind.KindMask) switch
    {
        SymbolKind.Feature => SemanticTokenTypes.Type,
        SymbolKind.Function => SemanticTokenTypes.Function,
        SymbolKind.Variable => SemanticTokenTypes.Variable,
        SymbolKind.Parameter => SemanticTokenTypes.Parameter,
        SymbolKind.Constant => ExtraSemanticTokenTypes.Constant,
        SymbolKind.Property => SemanticTokenTypes.Property,
        _ => null
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
}