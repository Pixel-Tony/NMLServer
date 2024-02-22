namespace NMLServer.Lexing;

internal abstract class BaseMulticharToken(int start, int end) : Token(start)
{
    public sealed override int end { get; } = end;
}