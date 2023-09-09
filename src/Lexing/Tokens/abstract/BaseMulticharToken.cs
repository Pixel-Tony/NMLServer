namespace NMLServer.Lexing.Tokens;

internal abstract class BaseMulticharToken : Token
{
    public readonly int End;

    protected BaseMulticharToken(int start, int end) : base(start)
    {
        End = end;
    }
}