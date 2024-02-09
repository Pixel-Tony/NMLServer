using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class FontGlyph : BaseSpriteHolder
{
    public FontGlyph(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}