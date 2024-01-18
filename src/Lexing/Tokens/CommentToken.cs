namespace NMLServer.Lexing.Tokens;

internal class CommentToken : MulticharToken
{
    public CommentToken(int start, int end) : base(start, end)
    { }
}