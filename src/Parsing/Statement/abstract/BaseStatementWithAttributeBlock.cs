using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class BaseStatementWithAttributeBlock : BaseStatementWithBlock
{
    protected readonly NMLAttribute[]? Attributes;

    protected BaseStatementWithAttributeBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            Attributes = NMLAttribute.TryParseManyInBlock(state, ref ClosingBracket);
        }
    }
}