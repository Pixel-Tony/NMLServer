using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal abstract partial class BaseSpriteHolder(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<BaseSpriteHolder.Sprite>(ref state, keyword);