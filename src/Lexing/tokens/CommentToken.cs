namespace NMLServer.Lexing;

internal sealed class CommentToken(int start, int end) : BaseMulticharToken(start, end);