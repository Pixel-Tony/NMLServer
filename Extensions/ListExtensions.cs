using System.Runtime.InteropServices;
using NMLServer.Model;

namespace NMLServer.Extensions;

internal static class ListExtensions
{
    public static List<T>? ToMaybeList<T>(this List<T> target) => target.Count > 0 ? target : null;

    public static int FindFirstNotBefore<T>(this List<T> source, int offset) where T : IHasEnd
    {
        ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(source);
        return span.FindFirstNotBefore(offset);
    }

    public static int FindLastBefore<T>(this List<T> source, int offset) where T : IHasEnd
    {
        ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(source);
        return span.FindLastBefore(offset);
    }

    public static int FindFirstAfter<T>(this List<T> source, int offset, int start = 0) where T : IHasStart
    {
        ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(source);
        return span.FindFirstAfter(offset, start);
    }

    public static int FindLastNotAfter<T>(this List<T> source, int offset, int end = -1) where T : IHasStart
    {
        ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(source);
        return span.FindLastNotAfter(offset, end);
    }

    public static int FindWhereStart<T>(this List<T> source, int offset, int end = -1) where T : IHasStart
    {
        ReadOnlySpan<T> span = CollectionsMarshal.AsSpan(source);
        return span.FindWhereStart(offset, end);
    }

    public static void ReplaceRange<T>(this List<T> destination, (int start, int end) range, in List<T> source)
    {
        var destCount = destination.Count;
        var srcCount = source.Count;
        var (start, end) = range;
        if (srcCount == 0)
        {
            destination.RemoveRange(start, end - start);
            return;
        }
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, destCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(end, 0);

        int delta = srcCount - (end - start);
        Span<T> destSpan;
        var newCount = destCount + delta;
        switch (delta)
        {
            case > 0:
                destination.EnsureCapacity(newCount);
                CollectionsMarshal.SetCount(destination, newCount);
                destSpan = MemoryMarshal.CreateSpan(ref CollectionsMarshal.AsSpan(destination)[0], destination.Capacity);
                destSpan[end..destCount].CopyTo(destSpan[(end + delta)..]);
                break;

            case < 0:
                destination.RemoveRange(end + delta, -delta);
                destSpan = CollectionsMarshal.AsSpan(destination);
                break;

            default:
                destSpan = CollectionsMarshal.AsSpan(destination);
                break;
        }
        CollectionsMarshal.AsSpan(source).CopyTo(destSpan[start..]);
        CollectionsMarshal.SetCount(destination, newCount);
    }
}