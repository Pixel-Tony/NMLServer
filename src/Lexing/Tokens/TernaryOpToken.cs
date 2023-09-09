namespace NMLServer.Lexing.Tokens;

internal class TernaryOpToken : BaseSingleCharacterToken, IHasPrecedence
{
    public int precedence => Grammar.TernaryOperatorPrecedence;

    public TernaryOpToken(int position) : base(position)
    { }
}