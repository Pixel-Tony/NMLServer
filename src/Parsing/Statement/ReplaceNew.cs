using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class ReplaceNew : BaseSpriteHolder
{
    public ReplaceNew(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}