namespace NMLServer.Lexing.Tokens;

internal abstract class ValueToken : MulticharToken
{
    protected ValueToken(int start, int end) : base(start, end)
    { }
}