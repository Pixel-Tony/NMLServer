namespace NMLServer.Lexing;

internal sealed class RangeToken(int start) : Token(start)
{
    public override int end => start + 2;

    public override int length => 2;
}