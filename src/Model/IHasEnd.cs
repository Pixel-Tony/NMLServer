namespace NMLServer.Model;

internal interface IHasEnd
{
    public int End { get; }

    public static int? LastOf<T1, T2>(List<T1>? first, List<T2>? second) where T1 : IHasEnd where T2 : IHasEnd
    {
        int? f = first?[^1].End;
        int? s = second?[^1].End;
        if (f is null)
            return s;
        return int.Max(f.Value, s ?? 0);
    }
}