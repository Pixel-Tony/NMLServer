using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class RecolourSprite(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<RecolourSprite.Line>(ref state, keyword);