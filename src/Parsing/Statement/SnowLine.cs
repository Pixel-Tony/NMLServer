using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class SnowLine : BaseStatementWithAttributeBlock
{
    public SnowLine(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}