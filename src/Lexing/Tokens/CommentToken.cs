namespace NMLServer.Lexing.Tokens;

internal class CommentToken : BaseRecordingToken
{
    public CommentToken(int start, int end) : base(start, end)
    { }
}