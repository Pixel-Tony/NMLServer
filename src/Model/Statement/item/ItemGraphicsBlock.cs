using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class ItemGraphicsBlock : BaseStatementWithBlockOf<ItemGraphicsAttribute>
{
    public ItemGraphicsBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}