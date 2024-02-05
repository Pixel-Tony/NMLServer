using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class IfBlock : BaseBlockWithChildren
{
    public IfBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}