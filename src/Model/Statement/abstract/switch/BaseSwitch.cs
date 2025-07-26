using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal abstract partial class BaseSwitch(
    ref ParsingState state,
    KeywordToken keyword,
    BlockStatement.ParamInfo info)
    : BlockStatement<BaseSwitch.Line>(ref state, keyword, info);