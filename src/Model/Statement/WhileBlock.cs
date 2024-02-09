using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class WhileBlock : BaseBlockWithChildren
{
    public WhileBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}