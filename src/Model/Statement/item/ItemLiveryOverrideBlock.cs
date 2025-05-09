using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class ItemLiveryOverrideBlock(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<ItemGraphicsAttribute>(ref state, keyword);