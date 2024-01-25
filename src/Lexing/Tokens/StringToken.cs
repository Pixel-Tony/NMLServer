namespace NMLServer.Lexing.Tokens;

internal sealed class StringToken : ValueToken
{
    public StringToken(int start, int end) : base(start, end)
    { }
}