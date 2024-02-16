namespace NMLServer.Lexing;

internal abstract class Token : IHasEnd
{
    public readonly int Start;

    public virtual int end => Start + 1;

    protected Token(int start)
    {
        Start = start;
    }
}