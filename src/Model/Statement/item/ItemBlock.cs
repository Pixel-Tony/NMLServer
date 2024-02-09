using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class ItemBlock : BaseBlockWithChildren
{
    public ItemBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}