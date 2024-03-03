using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class WhileBlock(ParsingState state, KeywordToken keyword) : BaseBlockWithChildren(state, keyword);