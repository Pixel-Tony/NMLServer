using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class AlternativeSprites : BaseSpriteHolder
{
    public AlternativeSprites(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}