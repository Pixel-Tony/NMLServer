using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class SnowLine : BaseStatementWithAttributeBlock
{
    public SnowLine(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}