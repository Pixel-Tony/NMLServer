using NMLServer.Model.Grammar;

namespace NMLServer.Extensions;

internal static class EnumExtensions
{
    public static bool HasFlagX(this SymbolKind v, SymbolKind flag) => (v & flag) != 0;

    public static int Precedence(this OperatorType type) => ((int)type) >> 8;
}