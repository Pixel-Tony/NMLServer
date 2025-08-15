using NMLServer.Model.Grammar;

namespace NMLServer.Model.Tokens;

internal sealed class IdentifierToken(int start, int end, SymbolKind kind) : BaseValueToken(start, end)
{
    public readonly SymbolKind Kind = kind;

    public override string? SemanticType { get; } = NML.GetSemanticTokenType(kind);
}