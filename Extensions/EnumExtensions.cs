using NMLServer.Model.Grammar;

namespace NMLServer.Extensions;

internal static class EnumExtensions
{
    public static int Precedence(this OperatorType type) => ((int)type) >> 4;
}