using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class SnowLine : BaseStatementWithAttributeBlock
{
    public SnowLine(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}