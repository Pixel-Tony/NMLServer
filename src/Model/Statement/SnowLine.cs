using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class SnowLine(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<NMLAttribute>(ref state, keyword, new ParamInfo(1, 1, -1, true));