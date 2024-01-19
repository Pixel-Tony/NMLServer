using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ItemGraphicsBlock : BaseStatementWithBlock
{
    private ItemGraphicsAttribute[]? _attributes;

    public ItemGraphicsBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            _attributes = ItemGraphicsAttribute.TryParseManyInBlock(state, ref ClosingBracket);
        }
    }
}