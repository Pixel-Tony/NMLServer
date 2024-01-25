namespace NMLServer.Lexing.Tokens;

internal sealed class IdentifierToken : ValueToken
{
    public IdentifierToken(int start, int end) : base(start, end)
    { }
}