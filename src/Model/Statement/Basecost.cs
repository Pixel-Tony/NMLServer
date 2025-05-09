using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class Basecost(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<NMLAttribute>(ref state, keyword);