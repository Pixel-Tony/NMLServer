using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class IdentifierToken(int start, int end, SymbolKind kind) : BaseValueToken(start, end)
{
    public readonly SymbolKind Kind = kind;

    internal override string? semanticType => (Kind & SymbolKind.KindMask) switch
    {
        SymbolKind.Function => SemanticTokenTypes.Function,
        SymbolKind.Feature => SemanticTokenTypes.Type,
        SymbolKind.Variable => SemanticTokenTypes.Variable,
        SymbolKind.Constant => SemanticTokenTypes.EnumMember,
        SymbolKind.Parameter => SemanticTokenTypes.Parameter,
        _ => null
    };
}