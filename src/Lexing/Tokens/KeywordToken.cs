namespace NMLServer.Lexing.Tokens;

internal class KeywordToken : Token
{
    public readonly string Type;

    public KeywordToken(string type)
    {
        Type = type;
    }
}