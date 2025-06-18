using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class ItemLiveryOverrideBlock(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<ItemGraphicsAttribute>(ref state, keyword, new ParamInfo(1, 1, -1, true));