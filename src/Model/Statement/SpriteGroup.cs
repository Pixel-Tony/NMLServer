using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class SpriteGroup : BaseStatementWithBlockOf<NMLAttribute>
{
    public SpriteGroup(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}