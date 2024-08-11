using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class IfBlock(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword)
    : InnerStatementNode(parent, ref state, keyword);