using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class Basecost : BaseStatementWithAttributeBlock
{
    public Basecost(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}