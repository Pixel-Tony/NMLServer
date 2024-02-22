namespace NMLServer.Lexing;

internal abstract class Token(int start) : IHasStart, IHasEnd
{
    public int start { get; set; } = start;

    public virtual int length => end - start;

    public virtual int end => start + 1;
}