namespace NMLServer.Lexing.Tokens;

internal class UnaryOpToken : Token
{
    public readonly char Sign;

    public UnaryOpToken(int pos) : base(pos)
    {
        Sign = Input[pos];
    }
}