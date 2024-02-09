using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Template : BaseSpriteHolder
{
    public Template(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}