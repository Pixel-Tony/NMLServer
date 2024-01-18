namespace NMLServer.Lexing.Tokens;

internal sealed class FailedToken : Token
{
    public FailedToken(int pos) : base(pos)
    { }
}