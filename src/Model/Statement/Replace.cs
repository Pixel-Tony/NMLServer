using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

// TODO requires special parameter handling (see also other sprite replacement statements)
internal sealed class Replace(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword, new ParamInfo(false, (1, 1)));