namespace NMLServer.Lexing.Tokens;

internal abstract class MulticharToken : Token
{
    public readonly int End;

    protected MulticharToken(int start, int end) : base(start)
    {
        End = end;
    }
}