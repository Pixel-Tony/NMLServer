namespace NMLServer.Lexing.Tokens;

internal class UnaryOpToken : BaseSingleCharacterToken
{
    public readonly char Sign;

    public UnaryOpToken(int pos) : base(pos)
    {
        Sign = Input[pos];
    }
}