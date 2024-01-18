namespace NMLServer.Lexing.Tokens;

internal class TernaryOpToken : Token
{
    public const int Precedence = Grammar.TernaryOperatorPrecedence;

    public TernaryOpToken(int position) : base(position)
    { }
}