namespace NMLServer.Lexing;

internal sealed class BracketToken : Token
{
    public readonly char Bracket;

    public BracketToken(int pos, char bracket) : base(pos)
    {
        Bracket = bracket;
    }

    public override string ToString() => Bracket.ToString();
}