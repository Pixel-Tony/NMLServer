using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class Cargotable(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<ValueWithComma<IdentifierToken>>(ref state, keyword);