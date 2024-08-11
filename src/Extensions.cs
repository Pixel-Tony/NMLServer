using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

// TODO docstrings
namespace NMLServer;

internal static class RangeExtensions
{
    public static Range RangeFromRaw(int line, int @char, int length) => new(line, @char, line, @char + length);
}

internal static class SpanExtensions
{
    public static Span<T> AsSpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);

    private static Span<T> CreateFullListSpan<T>(List<T> list)
        => MemoryMarshal.CreateSpan(ref CollectionsMarshal.AsSpan(list)[0], list.Capacity);

    // TODO: docstring
    public static void ReplaceRange<T>(this List<T> destination, List<T> source, int rangeStart, int rangeEnd)
    {
        int delta = source.Count - (rangeEnd - rangeStart);
        Span<T> destinationAsSpan;

        switch (delta)
        {
            case > 0:
                destination.EnsureCapacity(destination.Count + delta);
                destinationAsSpan = CreateFullListSpan(destination);
                destinationAsSpan[rangeEnd..destination.Count].CopyTo(destinationAsSpan[(rangeEnd + delta)..]);
                CollectionsMarshal.AsSpan(source).CopyTo(destinationAsSpan[rangeStart..]);
                CollectionsMarshal.SetCount(destination, destination.Count + delta);
                return;

            case < 0:
                destinationAsSpan = CollectionsMarshal.AsSpan(destination);
                destination.RemoveRange(rangeEnd + delta, -delta);
                CollectionsMarshal.AsSpan(source).CopyTo(destinationAsSpan[rangeStart..]);
                return;

            default:
                destinationAsSpan = CollectionsMarshal.AsSpan(destination);
                CollectionsMarshal.AsSpan(source).CopyTo(destinationAsSpan[rangeStart..]);
                return;
        }
    }

    public static void ComplementStartByRange<T>(this List<T> destination, in List<T> source, int end)
    {
        if (end is 0)
        {
            destination.InsertRange(0, source);
            return;
        }
        var capacity = destination.Count - end + source.Count;
        destination.EnsureCapacity(capacity);
        var span = CreateFullListSpan(destination);
        span[end..destination.Count].CopyTo(span[source.Count..]);
        CollectionsMarshal.AsSpan(source).CopyTo(span);
        CollectionsMarshal.SetCount(destination, capacity);
    }

    // note: destination.Count >= 1
    public static void ComplementEndByRange<T>(this List<T> destination, in List<T> source, int start)
    {
        var capacity = start + source.Count;
        destination.EnsureCapacity(capacity);
        var span = CreateFullListSpan(destination);
        CollectionsMarshal.AsSpan(source).CopyTo(span[start..]);
        CollectionsMarshal.SetCount(destination, capacity);
    }
}

internal static class Extensions
{
    public static List<T>? ToMaybeList<T>(this List<T> target)
        => target.Count > 0
            ? target
            : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidIdentifierCharacter(this char c) => c is '_' || char.IsLetterOrDigit(c);

    public static int LastOf<T1, T2>(IReadOnlyList<T1>? first, IReadOnlyList<T2>? second)
        where T1 : IHasEnd
        where T2 : IHasEnd
        => int.Max(first?[^1].end ?? 0, second?[^1].end ?? 0);

    public static SemanticTokenType? ToSemanticType(this SymbolKind target)
    {
        return (target & (SymbolKind)0x0F) switch
        {
            SymbolKind.Feature => SemanticTokenType.Type,
            SymbolKind.Switch => SemanticTokenType.Function,
            SymbolKind.Macro => SemanticTokenType.Macro,
            SymbolKind.Variable => SemanticTokenType.Variable,
            SymbolKind.Parameter => SemanticTokenType.Parameter,
            SymbolKind.Constant => SemanticTokenType.EnumMember,
            _ => null as SemanticTokenType?
        };
    }
}