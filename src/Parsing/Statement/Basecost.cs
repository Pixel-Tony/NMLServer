using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class Basecost : BaseStatementWithBlock
{
    private NMLAttribute[]? _attributes;

    public Basecost(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            _attributes = AttributesBlock.TryParseManyInBlock(state, ref ClosingBracket);
        }
    }
}