using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class RandomSwitch(ref ParsingState state, KeywordToken keyword)
    : BaseSwitch(ref state, keyword, new ParamInfo(3, 4, 2, true));