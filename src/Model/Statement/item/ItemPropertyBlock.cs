using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class ItemPropertyBlock(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<NMLAttribute>(ref state, keyword, ParamInfo.None);