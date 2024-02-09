using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Basecost : BaseStatementWithAttributeBlock
{
    public Basecost(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}