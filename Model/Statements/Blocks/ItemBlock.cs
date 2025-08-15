using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class ItemBlock(ref ParsingState state, KeywordToken keyword) : BaseParentStatement(ref state, keyword);