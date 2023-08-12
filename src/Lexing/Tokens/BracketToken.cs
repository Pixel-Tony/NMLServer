namespace NMLServer.Lexing.Tokens;

internal class BracketToken : Token
{
    public readonly char Bracket;
    
    public BracketToken(char bracket)
    {
        Bracket = bracket;
    }

    public override string ToString()
    {
        return Bracket.ToString();
    }
}