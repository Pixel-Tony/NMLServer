using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class BaseGraphics : BaseSpriteHolder
{
    public BaseGraphics(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}