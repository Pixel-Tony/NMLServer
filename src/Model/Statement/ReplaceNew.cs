using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class ReplaceNew : BaseSpriteHolder
{
    public ReplaceNew(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}