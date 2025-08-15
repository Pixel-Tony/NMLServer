using NMLServer.Model;

namespace NMLServer.Extensions;

internal static class SpanExtensions
{
    public static int FindFirstNotBefore<T>(this ReadOnlySpan<T> source, int offset) where T : IHasEnd
    {
        var result = source.Length;
        for (int left = 0, right = source.Length - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            if (source[mid].End <= offset)
            {
                left = mid + 1;
                continue;
            }
            result = mid;
            right = mid - 1;
        }
        return result;
    }

    public static int FindLastBefore<T>(this ReadOnlySpan<T> source, int offset) where T : IHasEnd
    {
        var result = -1;
        for (int left = 0, right = source.Length - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            if (source[mid].End >= offset)
            {
                right = mid - 1;
                continue;
            }
            result = mid;
            left = mid + 1;
        }
        return result;
    }

    public static int FindFirstAfter<T>(this ReadOnlySpan<T> source, int offset, int start = 0) where T : IHasStart
    {
        var result = source.Length;
        for (int left = start, right = source.Length - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            if (source[mid].Start <= offset)
            {
                left = mid + 1;
                continue;
            }
            result = mid;
            right = mid - 1;
        }
        return result;
    }

    public static int FindLastNotAfter<T>(this ReadOnlySpan<T> source, int offset, int end = int.MaxValue) where T : IHasStart
    {
        var result = -1;
        for (int left = 0, right = int.Min(end, source.Length - 1); left <= right;)
        {
            var mid = left + (right - left) / 2;
            if (source[mid].Start >= offset)
            {
                right = mid - 1;
                continue;
            }
            result = mid;
            left = mid + 1;
        }
        return result;
    }

    public static int FindWhereStart<T>(this ReadOnlySpan<T> source, int offset, int end = int.MaxValue) where T : IHasStart
    {
        for (int left = 0, right = int.Min(end, source.Length - 1); left <= right;)
        {
            var mid = left + (right - left) / 2;
            var midOffset = source[mid].Start;
            if (midOffset > offset)
            {
                right = mid - 1;
                continue;
            }
            if (midOffset < offset)
            {
                left = mid + 1;
                continue;
            }
            return mid;
        }
        return -1;
    }
}