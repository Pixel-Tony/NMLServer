using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class BaseGraphics(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword);