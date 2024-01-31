namespace NMLServer.Lexing.Tokens;

internal sealed class StringToken : BaseValueToken
{
    public StringToken(int start, int end) : base(start, end)
    { }
}