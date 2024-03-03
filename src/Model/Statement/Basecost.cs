using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Basecost(ParsingState state, KeywordToken keyword)
    : BaseStatementWithBlockOf<NMLAttribute>(state, keyword);