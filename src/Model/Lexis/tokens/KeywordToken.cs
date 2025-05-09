using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class KeywordToken(int start, int end, KeywordType type, KeywordKind kind)
    : BaseMulticharToken(start, end)
{
    public readonly KeywordType Type = type;
    public readonly KeywordKind Kind = kind;

    internal override string semanticType => SemanticTokenTypes.Keyword;
}