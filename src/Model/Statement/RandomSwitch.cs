using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class RandomSwitch : BaseSwitch
{
    public RandomSwitch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}