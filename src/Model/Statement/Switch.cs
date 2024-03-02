using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Switch(ParsingState state, KeywordToken keyword) : BaseSwitch(state, keyword);