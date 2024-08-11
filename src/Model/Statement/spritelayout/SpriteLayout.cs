using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed partial class SpriteLayout(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<SpriteLayout.Entry>(ref state, keyword);