namespace NMLServer.Lexing.Tokens;

internal abstract class BaseValueToken : BaseMulticharToken
{
    protected BaseValueToken(int start, int end) : base(start, end)
    { }
}