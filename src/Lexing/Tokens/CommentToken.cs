namespace NMLServer.Lexing.Tokens;

internal sealed class CommentToken : BaseMulticharToken
{
    public CommentToken(int start, int end) : base(start, end)
    { }
}