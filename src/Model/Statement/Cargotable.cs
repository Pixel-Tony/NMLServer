using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class Cargotable(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<ValueWithComma<BaseValueToken>>(ref state, keyword, ParamInfo.None);