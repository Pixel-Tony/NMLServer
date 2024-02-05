namespace NMLServer.Lexing.Tokens;

internal abstract class BaseMulticharToken : Token
{
    public readonly int Length;

    protected BaseMulticharToken(int start, int end) : base(start)
    {
        Length = end - start;
    }
}