using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseStatementWithAttributeBlock : BaseStatementWithBlock
{
    protected readonly IReadOnlyList<NMLAttribute>? Attributes;

    protected BaseStatementWithAttributeBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            Attributes = NMLAttribute.TryParseSomeInBlock(state, ref ClosingBracket);
        }
    }
}