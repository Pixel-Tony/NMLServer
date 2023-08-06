namespace NMLServer.Lexing.Tokens;

internal class BracketToken : Token
{
    public readonly char Bracket;
    
    public BracketToken(char bracket)
    {
        Bracket = _brackets.Contains(bracket)
            ? bracket
            : throw new ArgumentException("Not a bracket: " + bracket);
    }

    private static readonly HashSet<char> _brackets = new("(){}[]");
}