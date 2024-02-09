using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class ItemPropertyBlock : BaseStatementWithAttributeBlock
{
    public ItemPropertyBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}