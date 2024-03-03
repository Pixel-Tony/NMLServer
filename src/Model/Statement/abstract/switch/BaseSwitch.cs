using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract partial class BaseSwitch(ParsingState state, KeywordToken keyword)
    : BaseStatementWithBlockOf<BaseSwitch.Line>(state, keyword);