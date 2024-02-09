using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Spriteset : BaseSpriteHolder
{
    public Spriteset(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}