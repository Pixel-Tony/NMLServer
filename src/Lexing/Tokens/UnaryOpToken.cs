namespace NMLServer.Lexing.Tokens;

internal class UnaryOpToken : Token
{
    public readonly char Sign;

    public UnaryOpToken(char token)
    {
        Sign = token;
    }
}