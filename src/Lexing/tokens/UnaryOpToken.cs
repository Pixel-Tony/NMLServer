namespace NMLServer.Lexing;

internal sealed class UnaryOpToken : Token
{
    public readonly char Sign;

    public UnaryOpToken(int pos, char sign) : base(pos)
    {
        Sign = sign;
    }
}