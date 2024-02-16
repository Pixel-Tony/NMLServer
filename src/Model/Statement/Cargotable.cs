using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Cargotable : BaseStatementWithBlockOf<ValueWithComma<IdentifierToken>>
{
    public Cargotable(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}