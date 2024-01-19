using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ItemBlock : BaseParametrizedBlockWithChildren
{
    public ItemBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}