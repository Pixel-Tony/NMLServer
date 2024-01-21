using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class SpriteGroup : BaseStatementWithAttributeBlock
{
    public SpriteGroup(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}