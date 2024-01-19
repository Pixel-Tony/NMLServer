using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class ItemBlock : BaseParametrizedStatementWithBlock
{
    private readonly NMLFile _children;

    public ItemBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        _children = new NMLFile(state, true);
        ClosingBracket = state.ExpectClosingCurlyBracket();
    }
}