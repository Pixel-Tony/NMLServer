using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class Spriteset(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword, new ParamInfo(1, 2, 0, true));