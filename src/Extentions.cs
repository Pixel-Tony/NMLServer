namespace NMLServer;

internal static class ExtentionMethods
{
    public static T[]? ToMaybeArray<T>(this List<T> target) =>
        target.Count > 0
            ? target.ToArray()
            : null;
}