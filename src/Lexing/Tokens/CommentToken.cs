namespace NMLServer.Lexing.Tokens;

internal class CommentToken : BaseMulticharToken
{
    public CommentToken(int start, int end) : base(start, end)
    { }
}