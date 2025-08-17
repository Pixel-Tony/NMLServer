using System.Diagnostics;
using System.Runtime.InteropServices;
using NMLServer.Model;

namespace NMLServer.Extensions;

internal static class ListExtensions
{
    public static T Pop<T>(this List<T> stackLike)
    {
        var i = stackLike.Count - 1;
        T last = stackLike[i];
        stackLike.RemoveAt(i);
        return last;
    }

    public static List<T>? ToMaybeList<T>(this List<T> target) => target.Count > 0 ? target : null;

    public static int FindFirstNotBefore<T>(this List<T> source, int offset) where T : IHasEnd
        => source.ToReadOnlySpan().FindFirstNotBefore(offset);

    public static int FindLastBefore<T>(this List<T> source, int offset) where T : IHasEnd
        => source.ToReadOnlySpan().FindLastBefore(offset);

    public static int FindFirstAfter<T>(this List<T> source, int offset, int start = 0) where T : IHasStart
        => source.ToReadOnlySpan().FindFirstAfter(offset, start);

    public static int FindLastNotAfter<T>(this List<T> source, int offset, int end = int.MaxValue) where T : IHasStart
        => source.ToReadOnlySpan().FindLastNotAfter(offset, end);

    public static int FindWhereStart<T>(this List<T> source, int offset, int end = int.MaxValue) where T : IHasStart
        => source.ToReadOnlySpan().FindWhereStart(offset, end);

    public static void ReplaceRange<T>(this List<T> destination, List<T> source, int start, int end = int.MaxValue)
    {
        int destCount = destination.Count;
        Debug.Assert(start <= end);
        Debug.Assert(start <= destCount);
        Debug.Assert(end >= 0);

        end = int.Min(end, destCount);
        int delta = source.Count - (end - start);
        int newEnd = end + delta;
        int newCount = destCount + delta;

        if (newCount is 0)
            goto label_Crop;
        if (newCount > destCount)
            CollectionsMarshal.SetCount(destination, newCount);
        Span<T> destSpan = MemoryMarshal.CreateSpan(ref CollectionsMarshal.AsSpan(destination)[0], destination.Capacity);
        destSpan[end..destCount].CopyTo(destSpan[newEnd..newCount]);
        source.CopyTo(destSpan[start..newEnd]);
    label_Crop:
        CollectionsMarshal.SetCount(destination, newCount);
    }

    public static ReadOnlySpan<T> ToReadOnlySpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);
}