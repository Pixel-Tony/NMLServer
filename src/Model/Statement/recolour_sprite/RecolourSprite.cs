using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class RecolourSprite(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<RecolourSprite.Line>(ref state, keyword, ParamInfo.None);