namespace NMLServer.Lexing.Tokens;

internal sealed class BracketToken : Token
{
    public readonly char Bracket;

    public BracketToken(int pos) : base(pos)
    {
        Bracket = Input[pos];
    }

    public override string ToString() => Bracket.ToString();
}