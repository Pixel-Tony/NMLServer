using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed class Switch : BaseSwitch
{
    public Switch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}