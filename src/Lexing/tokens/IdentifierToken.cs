using System.Runtime.CompilerServices;

namespace NMLServer.Lexing;

internal sealed class IdentifierToken(int start, int end, StringView value) : BaseValueToken(start, end)
{
    public SymbolKind kind { get; set; } = Grammar.GetSymbolKind(value);

    public readonly int Hash = string.GetHashCode(value);

    public bool Equals(IdentifierToken obj, StringView context)
    {
        var value = Value(context);
        var tokenValue = obj.Value(context);
        return value.Equals(tokenValue, StringComparison.Ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private StringView Value(StringView source) => source.Slice(start, length);
}