using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class ItemBlock(BaseParentStatement? parent, ref ParsingState state, KeywordToken keyword)
    : BaseParentStatement(parent, ref state, keyword);