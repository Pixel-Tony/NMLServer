using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class IfBlock : BaseParametrizedBlockWithChildren
{
    public IfBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}