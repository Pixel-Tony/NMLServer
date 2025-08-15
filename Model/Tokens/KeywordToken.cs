using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Tokens;

internal sealed class KeywordToken(int start, int end, Keyword keyword)
    : BaseMulticharToken(start, end)
{
    public readonly Keyword Keyword = keyword;

    public bool IsExpressionUsable => (Keyword & Keyword._Kind_ExpressionUsable) != 0;

    public bool IsDefiningStatement => (Keyword & Keyword._Kind_DefinesStatement) != 0;

    public uint MinParams => ((uint)(Keyword & Keyword._MinParams_Mask) >> (int)Keyword._MinParams_Shift) - 1;

    public uint MaxParams => ((uint)(Keyword & Keyword._MaxParams_Mask) >> (int)Keyword._MaxParams_Shift) - 1;

    public bool RequiresParens => (Keyword & Keyword._RequiresParens) != 0;

    public uint SymbolIndex => ((uint)(Keyword & Keyword._SymbolIndex_Mask) >> (int)Keyword._SymbolIndex_Shift) - 1;

    public SymbolKind SymbolKind => (SymbolKind)(Keyword & Keyword._SymbolKind_Mask);

    public override string SemanticType => SemanticTokenTypes.Keyword;
}