namespace NMLServer.Lexing.Tokens;

internal class IdentifierToken : ValueToken
{
    public IdentifierToken(int start, int end) : base(start, end)
    { }
}