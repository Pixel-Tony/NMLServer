using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class BaseGraphics(ParsingState state, KeywordToken keyword) : BaseSpriteHolder(state, keyword);