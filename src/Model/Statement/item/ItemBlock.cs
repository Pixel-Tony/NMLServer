using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class ItemBlock(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword)
    : InnerStatementNode(parent, ref state, keyword, new ParamInfo(true, (1, 4), (1, SymbolKind.Variable)));