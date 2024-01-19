using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ReplaceNew : BaseSpriteHolder
{
    public ReplaceNew(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}