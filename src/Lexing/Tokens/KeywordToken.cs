using static NMLServer.Grammar;

namespace NMLServer.Lexing.Tokens;

internal class KeywordToken : BaseMulticharToken
{
    public readonly KeywordType Type;
    public readonly bool IsBlock;
    public readonly bool IsExpressionUsable;

    public KeywordToken(int start, int end, KeywordType type) : base(start, end)
    {
        Type = type;
        IsBlock = BlockKeywordTypes.Contains(Type);
        IsExpressionUsable = ExpressionKeywordTypes.Contains(Type);
    }
}