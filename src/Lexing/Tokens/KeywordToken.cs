namespace NMLServer.Lexing.Tokens;

internal class KeywordToken : BaseRecordingToken
{
    public KeywordToken(string type) : base(type)
    { }

    public KeywordToken(BaseRecordingToken another) : base(another)
    { }
}