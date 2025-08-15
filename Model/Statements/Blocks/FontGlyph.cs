using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

// TODO requires special parameter handling (see also other sprite replacement statements)
internal sealed class FontGlyph(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword);