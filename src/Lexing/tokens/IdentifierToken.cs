using System.Runtime.CompilerServices;

namespace NMLServer.Lexing;

internal sealed class IdentifierToken : BaseValueToken
{
    public SymbolKind kind { get; set; }

    public readonly int Hash;

    public IdentifierToken(int start, int end, ReadOnlySpan<char> value) : base(start, end)
    {
        kind = Grammar.GetSymbolKind(value);
        Hash = string.GetHashCode(value);
    }

    public bool Equals(IdentifierToken obj, ReadOnlySpan<char> context)
    {
        var value = Value(context);
        var tokenValue = obj.Value(context);
        return value.Equals(tokenValue, StringComparison.Ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<char> Value(ReadOnlySpan<char> source) => source.Slice(Start, Length);
}