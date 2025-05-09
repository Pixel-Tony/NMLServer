using NMLServer.Model.Expression;

namespace NMLServer.Model.Lexis;

internal abstract class Token(int start, int length) : IHasStart, IHasEnd
{
    public int start { get; set; } = start;

    public int end => start + length;

    public int length { get; } = length;

    internal abstract string? semanticType { get; }
}