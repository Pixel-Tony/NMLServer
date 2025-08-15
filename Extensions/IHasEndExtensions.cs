using NMLServer.Model;

namespace NMLServer.Extensions;

internal static class IHasEndExtensions
{
    public static int? LastOf<T1, T2>(this (List<T1>? first, List<T2>? second) pair)
        where T1 : IHasEnd
        where T2 : IHasEnd
    {
        int? f = pair.first?[^1].End;
        int? s = pair.second?[^1].End;
        if (f is null)
            return s;
        return int.Max(f.Value, s ?? 0);
    }
}