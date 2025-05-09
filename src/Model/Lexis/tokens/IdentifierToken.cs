using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class IdentifierToken(int start, int end, SymbolKind kind, int hash) : BaseValueToken(start, end)
{
    public readonly SymbolKind Kind = kind;
    public readonly int Hash = hash;

    public bool IsContextuallyEqual(IdentifierToken obj, StringView context)
        => context[start..end].SequenceEqual(context[obj.start..obj.end]);

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