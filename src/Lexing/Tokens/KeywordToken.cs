using static NMLServer.Grammar;

namespace NMLServer.Lexing.Tokens;

internal class KeywordToken : MulticharToken
{
    public readonly KeywordType Type;
    public readonly bool IsExpressionUsable;

    public KeywordToken(int start, int end, KeywordType type) : base(start, end)
    {
        Type = type;
        IsExpressionUsable = ExpressionKeywordTypes.Contains(Type);
    }
}