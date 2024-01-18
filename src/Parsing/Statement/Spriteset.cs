using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class Spriteset : BaseSpriteHolder
{
    public Spriteset(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}