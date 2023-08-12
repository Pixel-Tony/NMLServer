namespace NMLServer.Lexing.Tokens;

internal class CommentToken : BaseRecordingToken
{
    public CommentToken(string s) : base(s)
    { }

    public CommentToken(char c) : base(c)
    { }
}