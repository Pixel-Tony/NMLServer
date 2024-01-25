namespace NMLServer.Lexing.Tokens;

internal sealed class CommentToken : MulticharToken
{
    public CommentToken(int start, int end) : base(start, end)
    { }
}