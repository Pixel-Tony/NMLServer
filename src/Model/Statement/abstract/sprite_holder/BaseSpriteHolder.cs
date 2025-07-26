using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal abstract partial class BaseSpriteHolder(ref ParsingState state, KeywordToken keyword, BlockStatement.ParamInfo info)
    : BlockStatement<BaseSpriteHolder.Sprite>(ref state, keyword, info);