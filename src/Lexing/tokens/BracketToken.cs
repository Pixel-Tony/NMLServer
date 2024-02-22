namespace NMLServer.Lexing;

internal sealed class BracketToken(int start, char bracket) : Token(start)
{
    public readonly char Bracket = bracket;

    public override string ToString() => Bracket.ToString();
}