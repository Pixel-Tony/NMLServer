using static NMLServer.Grammar;

namespace NMLServer.Lexing.Tokens;

internal class KeywordToken : BaseRecordingToken
{
    public KeywordToken(int start, int end, string value) : base(start, end)
    {
        IsBlock = BlockKeywords.Contains(value);
        IsExpressionUsable = ExpressionKeywords.ContainsKey(value);
    }
    
    public readonly bool IsBlock;
    public readonly bool IsExpressionUsable;
}