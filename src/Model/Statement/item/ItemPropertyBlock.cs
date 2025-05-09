using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class ItemPropertyBlock(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<NMLAttribute>(ref state, keyword);