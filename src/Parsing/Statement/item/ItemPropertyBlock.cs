using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ItemPropertyBlock : BaseStatementWithAttributeBlock
{
    public ItemPropertyBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}