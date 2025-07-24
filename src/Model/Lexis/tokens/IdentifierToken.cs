using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class IdentifierToken(int start, int end, SymbolKind kind) : BaseValueToken(start, end)
{
    public SymbolKind Kind = kind;

    public override string? SemanticType => (Kind & SymbolKind.KindMask) switch
    {
        SymbolKind.Function => SemanticTokenTypes.Function,
        SymbolKind.Feature => SemanticTokenTypes.Type,
        SymbolKind.Variable => SemanticTokenTypes.Variable,
        SymbolKind.Constant => SemanticTokenTypes.EnumMember,
        SymbolKind.Parameter => SemanticTokenTypes.Parameter,
        _ => null
    };

    public CompletionItemKind CompletionItemKind => (Kind & SymbolKind.KindMask) switch
    {
        SymbolKind.Function => CompletionItemKind.Function,
        SymbolKind.Feature => CompletionItemKind.Class,
        SymbolKind.Variable => CompletionItemKind.Variable,
        SymbolKind.Constant => CompletionItemKind.EnumMember,
        SymbolKind.Parameter => CompletionItemKind.Variable,
        _ => CompletionItemKind.Text
    };
}