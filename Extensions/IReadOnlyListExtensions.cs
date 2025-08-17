using NMLServer.Model;

namespace NMLServer.Extensions;

internal static class IReadOnlyListExtensions
{
    public static int FindFirstAfter<T, TV>(this IReadOnlyList<T> source, TV offset, int start = 0)
        where T : IComparable<T, TV>
    {
        var result = source.Count;
        for (int left = start, right = result - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            if (source[mid].CompareTo(offset) <= 0)
            {
                left = mid + 1;
                continue;
            }
            result = mid;
            right = mid - 1;
        }
        return result;
    }

    public static int FindLastBefore<T, TV>(this IReadOnlyList<T> source, TV offset, int end = int.MaxValue)
        where T : IComparable<T, TV>
    {
        var result = -1;
        for (int left = 0, right = int.Min(end, source.Count - 1); left <= right;)
        {
            var mid = left + (right - left) / 2;
            if (source[mid].CompareTo(offset) >= 0)
            {
                right = mid - 1;
                continue;
            }
            result = mid;
            left = mid + 1;
        }
        return result;
    }

    public static int FindWhereOffset<T, TV>(this IReadOnlyList<T> source, TV offset, int start = 0, int end = int.MaxValue)
        where T : IComparable<T, TV>
    {
        for (int left = start, right = int.Min(end, source.Count - 1); left <= right;)
        {
            var mid = left + (right - left) / 2;
            switch (source[mid].CompareTo(offset))
            {
                case > 0:
                    right = mid - 1;
                    continue;
                case < 0:
                    left = mid + 1;
                    continue;
                default:
                    return mid;
            }
        }
        return -1;
    }
}