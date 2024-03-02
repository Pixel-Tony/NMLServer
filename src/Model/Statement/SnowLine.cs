using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class SnowLine(ParsingState state, KeywordToken keyword)
    : BaseStatementWithBlockOf<NMLAttribute>(state, keyword);