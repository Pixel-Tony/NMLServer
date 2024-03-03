using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class ItemLiveryOverrideBlock : BaseStatementWithBlockOf<ItemGraphicsAttribute>
{
    public ItemLiveryOverrideBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}