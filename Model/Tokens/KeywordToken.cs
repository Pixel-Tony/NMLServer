using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Tokens;

internal sealed class KeywordToken(int start, int end, Keyword keyword)
    : BaseMulticharToken(start, end)
{
    public readonly Keyword Keyword = keyword;

    public bool IsExpressionUsable => (Keyword & Keyword._Kind_ExpressionUsable) != 0;

    public bool IsDefiningStatement => (Keyword & Keyword._Kind_DefinesStatement) != 0;

    public override string SemanticType => SemanticTokenTypes.Keyword;
}