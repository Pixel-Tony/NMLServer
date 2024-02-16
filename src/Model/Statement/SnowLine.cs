using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class SnowLine : BaseStatementWithBlockOf<NMLAttribute>
{
    public SnowLine(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}