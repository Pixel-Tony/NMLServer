using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class IfBlock(ParsingState state, KeywordToken keyword) : BaseBlockWithChildren(state, keyword);