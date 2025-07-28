namespace NMLServer.Model.Lexis;

internal sealed class IdentifierToken(int start, int end, SymbolKind kind) : BaseValueToken(start, end)
{
    public SymbolKind Kind = kind;

    public override string? SemanticType { get; } = Grammar.GetSemanticTokenType(kind);
}