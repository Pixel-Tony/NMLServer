using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class ItemBlock : BaseBlockWithChildren
{
    public ItemBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}