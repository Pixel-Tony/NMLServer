namespace NMLServer.Lexing;

internal sealed class RangeToken : Token
{
    public override int end => Start + 2;

    public RangeToken(int start) : base(start)
    { }
}