using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract partial class BaseSwitch : BaseStatementWithBlockOf<BaseSwitch.Line>
{
    protected BaseSwitch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}