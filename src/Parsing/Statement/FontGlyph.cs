using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class FontGlyph : BaseSpriteHolder
{
    public FontGlyph(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}