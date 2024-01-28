namespace NMLServer;

internal static class Extentions
{
    public static T[]? ToArrayOrNull<T>(this List<T> target) => target.Count > 0
        ? target.ToArray()
        : null;
}