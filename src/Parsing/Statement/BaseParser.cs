using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing;

internal abstract class BaseParser
{
    protected static Token[] Tokens = null!;
    protected static int Pointer;
    protected static int Max;
    protected static List<Token> UnexpectedTokens = null!;

    public static void Use(Token[] tokens)
    {
        Tokens = tokens;
        Max = Tokens.Length;
        Pointer = 0;
        UnexpectedTokens = new List<Token>();
    }
}