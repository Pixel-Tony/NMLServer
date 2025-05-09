using System.Runtime.InteropServices;

namespace NMLServer.Extensions;

internal static class ListExtensions
{
    private static Span<T> MakeFullSpan<T>(this List<T> list)
        => MemoryMarshal.CreateSpan(ref CollectionsMarshal.AsSpan(list)[0], list.Capacity);

    public static List<T>? ToMaybeList<T>(this List<T> target) => target.Count > 0 ? target : null;

    public static void ReplaceRange<T>(this List<T> destination, List<T> source, int rangeStart, int rangeEnd)
    {
        int delta = source.Count - (rangeEnd - rangeStart);

        Span<T> destinationAsSpan;
        switch (delta)
        {
            case > 0:
                destination.EnsureCapacity(destination.Count + delta);
                destinationAsSpan = MakeFullSpan(destination);
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
        var capacity = destination.Count - end + source.Count;
        destination.EnsureCapacity(capacity);
        var span = MakeFullSpan(destination);
        span[end..destination.Count].CopyTo(span[source.Count..]);
        CollectionsMarshal.AsSpan(source).CopyTo(span);
        CollectionsMarshal.SetCount(destination, capacity);
    }

    public static void ComplementEndByRange<T>(this List<T> destination, in List<T> source, int start)
    {
        var capacity = start + source.Count;
        destination.EnsureCapacity(capacity);
        var span = MakeFullSpan(destination);
        CollectionsMarshal.AsSpan(source).CopyTo(span[start..]);
        CollectionsMarshal.SetCount(destination, capacity);
    }
}