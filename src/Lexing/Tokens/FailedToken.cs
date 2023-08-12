namespace NMLServer.Lexing.Tokens;

internal sealed class FailedToken : Token
{
    private readonly char _token;
    public FailedToken(char c) => _token = c;
}