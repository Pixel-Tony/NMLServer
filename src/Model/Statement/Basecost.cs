using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Basecost : BaseStatementWithBlockOf<NMLAttribute>
{
    public Basecost(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}