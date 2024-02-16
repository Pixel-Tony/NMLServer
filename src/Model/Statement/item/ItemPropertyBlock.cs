using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class ItemPropertyBlock : BaseStatementWithBlockOf<NMLAttribute>
{
    public ItemPropertyBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}