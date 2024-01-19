using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ItemLiveryOverrideBlock : BaseParametrizedStatementWithBlock
{
    private ItemGraphicsAttribute[]? _attributes;

    public ItemLiveryOverrideBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            _attributes = ItemGraphicsAttribute.TryParseManyInBlock(state, ref ClosingBracket);
        }
    }
}