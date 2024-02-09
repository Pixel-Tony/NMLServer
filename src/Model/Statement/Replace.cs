using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Replace : BaseSpriteHolder
{
    public Replace(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}