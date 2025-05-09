using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class ItemGraphicsBlock(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<ItemGraphicsAttribute>(ref state, keyword);