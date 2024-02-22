namespace NMLServer.Lexing;

internal sealed class UnaryOpToken(int start, char sign) : Token(start)
{
    public readonly char Sign = sign;
}