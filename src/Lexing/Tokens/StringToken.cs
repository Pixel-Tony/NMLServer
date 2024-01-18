namespace NMLServer.Lexing.Tokens;

internal class StringToken : ValueToken
{
    public StringToken(int start, int end) : base(start, end)
    { }
}