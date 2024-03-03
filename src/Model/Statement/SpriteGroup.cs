using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class SpriteGroup(ParsingState state, KeywordToken keyword)
    : BaseStatementWithBlockOf<NMLAttribute>(state, keyword);