namespace NMLServer.Lexing;

internal abstract class BaseMulticharToken : Token
{
    public readonly int Length;

    public sealed override int end => Start + Length;

    protected BaseMulticharToken(int start, int end) : base(start)
    {
        Length = end - start;
    }
}