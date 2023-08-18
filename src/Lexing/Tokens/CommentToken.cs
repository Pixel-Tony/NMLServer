namespace NMLServer.Lexing.Tokens;

internal class CommentToken : BaseRecordingToken
{
    public CommentToken(char c) : base(c)
    { }

    public CommentToken(BaseRecordingToken other) : base(other)
    { }
}