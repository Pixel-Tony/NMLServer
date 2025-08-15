using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal class LiveryOverrideBlock(ref ParsingState state, KeywordToken keyword)
    : BaseSwitch(ref state, keyword);