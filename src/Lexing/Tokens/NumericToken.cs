namespace NMLServer.Lexing.Tokens;

internal class NumericToken : ValueToken
{
    public NumericToken(int start, int end) : base(start, end)
    { }
}