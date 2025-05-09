using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class SpriteLayout(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<SpriteLayout.Entry>(ref state, keyword);