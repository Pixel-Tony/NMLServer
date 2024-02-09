using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class AlternativeSprites : BaseSpriteHolder
{
    public AlternativeSprites(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}