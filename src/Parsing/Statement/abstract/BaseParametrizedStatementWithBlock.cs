using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseParametrizedStatementWithBlock : BaseParametrizedStatement
{
    protected BracketToken? OpeningBracket;
    protected BracketToken? ClosingBracket;

    protected BaseParametrizedStatementWithBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        var (isClosingInstead, bracket) = state.ExpectOpeningCurlyBracket();
        if (isClosingInstead)
        {
            ClosingBracket = bracket;
        }
        else
        {
            OpeningBracket = bracket;
        }
    }
}