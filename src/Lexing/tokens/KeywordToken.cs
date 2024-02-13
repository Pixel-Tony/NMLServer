namespace NMLServer.Lexing;

internal sealed class KeywordToken : BaseMulticharToken
{
    public readonly KeywordType Type;
    public readonly KeywordKind Kind;

    public KeywordToken(int start, int end, KeywordType type, KeywordKind kind) : base(start, end)
    {
        Type = type;
        Kind = kind;
    }
}