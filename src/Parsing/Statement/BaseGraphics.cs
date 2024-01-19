using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class BaseGraphics : BaseSpriteHolder
{
    public BaseGraphics(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}