using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed partial class SpriteLayout : BaseStatementWithBlockOf<SpriteLayout.Entry>
{
    public SpriteLayout(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}