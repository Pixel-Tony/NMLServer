using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ItemLiveryOverrideBlock : BaseStatementWithBlock
{
    private IReadOnlyList<ItemGraphicsAttribute>? _attributes;

    public ItemLiveryOverrideBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            _attributes = ItemGraphicsAttribute.TryParseSomeInBlock(state, ref ClosingBracket);
        }
    }
}