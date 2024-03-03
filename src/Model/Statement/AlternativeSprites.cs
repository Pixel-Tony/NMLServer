using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class AlternativeSprites(ParsingState state, KeywordToken keyword) : BaseSpriteHolder(state, keyword);