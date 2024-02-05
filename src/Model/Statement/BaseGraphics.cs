using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class BaseGraphics : BaseSpriteHolder
{
    public BaseGraphics(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}