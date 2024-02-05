using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class Basecost : BaseStatementWithAttributeBlock
{
    public Basecost(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}