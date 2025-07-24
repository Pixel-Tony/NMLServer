using System.Runtime.InteropServices;

namespace NMLServer.Extensions;

internal static class ListExtensions
{
    public static List<T>? ToMaybeList<T>(this List<T> target) => target.Count > 0 ? target : null;

    public static void ReplaceStart<T>(this List<T> destination, in List<T> source, int end)
        => ReplaceRange(destination, in source, (0, end));

    public static void ReplaceEnd<T>(this List<T> destination, in List<T> source, int start)
        => ReplaceRange(destination, in source, (start, destination.Count));

    public static void ReplaceRange<T>(this List<T> destination, in List<T> source, (int start, int end) range)
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