namespace NMLServer.Lexing.Tokens;

internal sealed class FailedToken : BaseSingleCharacterToken
{
    public FailedToken(int pos) : base(pos)
    { }
}