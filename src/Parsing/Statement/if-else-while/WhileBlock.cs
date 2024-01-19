using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class WhileBlock : BaseParametrizedBlockWithChildren
{
    public WhileBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}