using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class SnowLine(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<NMLAttribute>(ref state, keyword);