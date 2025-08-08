using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class AlternativeSprites(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword);