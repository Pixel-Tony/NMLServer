using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class ItemGraphicsBlock(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<ItemGraphicsAttribute>(ref state, keyword, ParamInfo.None);