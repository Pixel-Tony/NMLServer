using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class IfBlock : BaseBlockWithChildren
{
    public IfBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}