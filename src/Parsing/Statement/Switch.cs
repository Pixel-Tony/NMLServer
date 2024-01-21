using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class Switch : BaseSwitch
{
    public Switch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}