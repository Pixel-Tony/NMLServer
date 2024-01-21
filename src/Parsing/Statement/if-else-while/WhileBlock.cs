using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class WhileBlock : BaseBlockWithChildren
{
    public WhileBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}