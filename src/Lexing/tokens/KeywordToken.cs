namespace NMLServer.Lexing;

internal sealed class KeywordToken(int start, int end, KeywordType type, KeywordKind kind)
    : BaseMulticharToken(start, end)
{
    public readonly KeywordType Type = type;
    public readonly KeywordKind Kind = kind;
}