namespace NMLServer.Lexing.Tokens;

internal sealed class NumericToken : ValueToken
{
    public NumericToken(int start, int end) : base(start, end)
    { }
}