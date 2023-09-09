namespace NMLServer.Lexing.Tokens;

internal class BracketToken : BaseSingleCharacterToken
{
    public readonly char Bracket;

    public BracketToken(int start) : base(start)
    {
        Bracket = Input[start];
    }

    public override string ToString() => Bracket.ToString();
}