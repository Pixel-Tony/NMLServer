namespace NMLServer.Lexing.Tokens;

internal sealed class IdentifierToken : BaseValueToken
{
    public IdentifierToken(int start, int end) : base(start, end)
    { }
}