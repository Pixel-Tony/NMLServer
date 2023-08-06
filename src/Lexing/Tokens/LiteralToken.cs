namespace NMLServer.Lexing.Tokens;

internal class LiteralToken : BaseRecordingToken
{
    public LiteralToken(string s = "") : base(s)
    { }

    public LiteralToken(char c) : base(c)
    { }
}

internal class NumericToken : BaseRecordingToken
{
    public NumericToken(string s) : base(s)
    { }
}