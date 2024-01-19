using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class Replace : BaseSpriteHolder
{
    public Replace(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}