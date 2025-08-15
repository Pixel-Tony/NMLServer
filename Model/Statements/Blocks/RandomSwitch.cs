using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class RandomSwitch(ref ParsingState state, KeywordToken keyword)
    : BaseSwitch(ref state, keyword);