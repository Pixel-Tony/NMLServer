using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed partial class RecolourSprite : BaseStatementWithBlockOf<RecolourSprite.Line>
{
    public RecolourSprite(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}