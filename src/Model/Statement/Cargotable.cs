using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Cargotable(ParsingState state, KeywordToken keyword)
    : BaseStatementWithBlockOf<ValueWithComma<IdentifierToken>>(state, keyword);