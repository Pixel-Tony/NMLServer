using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal static class Extentions
{
    public static List<T>? ToMaybeList<T>(this List<T> target) => target.Count > 0
        ? target
        : null;

    public static SemanticTokenType ToGeneralTokenType(this SymbolKind target) => target switch
    {
        SymbolKind.Feature => SemanticTokenType.Type,
        SymbolKind.Function => SemanticTokenType.Function,
        SymbolKind.Macro => SemanticTokenType.Macro,
        SymbolKind.Variable => SemanticTokenType.Variable,
        SymbolKind.Parameter => SemanticTokenType.Parameter,
        SymbolKind.Constant => SemanticTokenType.EnumMember,
        _ => SemanticTokenType.Variable
    };
}