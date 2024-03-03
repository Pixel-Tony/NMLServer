using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class ElseBlock(ParsingState state, KeywordToken keyword) : BaseBlockWithChildren(state, keyword);