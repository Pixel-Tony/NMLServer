namespace NMLServer.Lexing;

internal sealed class IdentifierToken(int start, int end, StringView value) : BaseValueToken(start, end)
{
    public readonly int Hash = string.GetHashCode(value);
    public SymbolKind kind { get; set; } = Grammar.GetSymbolKind(new string(value));

    public bool ContextuallyEqual(IdentifierToken obj, StringView context)
    {
        var value = context.Slice(start, length);
        var tokenValue = context.Slice(obj.start, obj.length);
        return value.Equals(tokenValue, StringComparison.Ordinal);
    }
}