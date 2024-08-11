using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class WhileBlock(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword) : InnerStatementNode(parent, ref state, keyword);