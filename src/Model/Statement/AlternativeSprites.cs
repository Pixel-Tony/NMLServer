using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class AlternativeSprites(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword, new ParamInfo(3, 5, -1, true));