namespace NMLServer.Lexing;

internal sealed class UnknownToken : Token
{
    public UnknownToken(int pos) : base(pos)
    { }
}