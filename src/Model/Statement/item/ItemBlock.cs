using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class ItemBlock(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword)
    : InnerStatementNode(parent, ref state, keyword);