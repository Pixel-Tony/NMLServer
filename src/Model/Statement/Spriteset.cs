using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Spriteset(ParsingState state, KeywordToken keyword) : BaseSpriteHolder(state, keyword);