using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class RandomSwitch(ParsingState state, KeywordToken keyword) : BaseSwitch(state, keyword);