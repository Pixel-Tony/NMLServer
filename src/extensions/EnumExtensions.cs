using System.Runtime.CompilerServices;
using NMLServer.Model;

namespace NMLServer.Extensions;

internal static class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFlaggedBy(this SymbolKind a, SymbolKind flag) => (a & flag) != 0;
}