using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class Template : BaseSpriteHolder
{
    public Template(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}