using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class Switch(ref ParsingState state, KeywordToken keyword) : BaseSwitch(ref state, keyword);