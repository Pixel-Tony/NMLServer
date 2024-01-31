namespace NMLServer.Lexing.Tokens;

internal sealed class NumericToken : BaseValueToken
{
    public NumericToken(int start, int end) : base(start, end)
    { }
}