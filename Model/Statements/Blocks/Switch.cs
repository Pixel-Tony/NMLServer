using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class Switch(ref ParsingState state, KeywordToken keyword) : BaseSwitch(ref state, keyword);