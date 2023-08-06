namespace NMLServer.Lexing.Tokens;

internal sealed class FailedToken : Token
{
    public readonly char Token;
    public FailedToken(char c) => Token = c;
}