namespace NMLServer.Model;

internal interface IHasEnd
{
    public int end { get; }

    public static bool LastOf<T1, T2>(List<T1>? first, List<T2>? second, out int value)
        where T1 : IHasEnd
        where T2 : IHasEnd
        => (value = LastOf(first, second)) != 0;

    public static int LastOf<T1, T2>(List<T1>? first, List<T2>? second)
        where T1 : IHasEnd
        where T2 : IHasEnd
        => int.Max(first?[^1].end ?? 0, second?[^1].end ?? 0);
}