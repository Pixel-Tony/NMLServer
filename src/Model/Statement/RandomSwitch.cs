using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class RandomSwitch : BaseSwitch
{
    public RandomSwitch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}