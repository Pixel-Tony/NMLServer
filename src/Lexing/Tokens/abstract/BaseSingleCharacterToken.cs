namespace NMLServer.Lexing.Tokens;

internal abstract class BaseSingleCharacterToken : Token
{
    protected BaseSingleCharacterToken(int position) : base(position)
    { }
}