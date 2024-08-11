using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class SpriteGroup(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<NMLAttribute>(ref state, keyword);