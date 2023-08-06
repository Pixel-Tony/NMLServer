namespace NMLServer.Lexing.Tokens;

internal class UnaryOpToken : Token
{
    public readonly bool IsLogical;

    public UnaryOpToken(char token)
    {
        IsLogical = token == '!';
    }
}