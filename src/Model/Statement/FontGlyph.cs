using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class FontGlyph(ParsingState state, KeywordToken keyword) : BaseSpriteHolder(state, keyword);