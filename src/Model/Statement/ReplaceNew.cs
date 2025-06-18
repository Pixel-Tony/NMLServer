using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

// TODO requires special parameter handling (see also other sprite replacement statements)
internal sealed class ReplaceNew(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword, new ParamInfo(1, 1, -1, false));