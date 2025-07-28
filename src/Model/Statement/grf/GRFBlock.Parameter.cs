using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal partial class GRFBlock
{
    private sealed partial class Parameter(ref ParsingState state, KeywordToken keyword)
        : BlockStatement<Parameter.Block>(ref state, keyword, new ParamInfo(false, (0, 1)));
}