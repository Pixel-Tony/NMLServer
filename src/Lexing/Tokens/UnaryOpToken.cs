namespace NMLServer.Lexing.Tokens;

internal sealed class UnaryOpToken : Token
{
    public readonly char Sign;

    public UnaryOpToken(int pos) : base(pos)
    {
        Sign = Input[pos];
    }
}