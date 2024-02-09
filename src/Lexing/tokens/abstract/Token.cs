namespace NMLServer.Lexing;

internal abstract class Token
{
    public readonly int Start;

    protected Token(int start)
    {
        Start = start;
    }
}