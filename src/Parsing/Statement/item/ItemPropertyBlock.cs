using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ItemPropertyBlock : BaseStatementWithBlock
{
    private NMLAttribute[]? _attributes;

    public ItemPropertyBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            _attributes = NMLAttribute.TryParseManyInBlock(state, ref ClosingBracket);
        }
    }
}