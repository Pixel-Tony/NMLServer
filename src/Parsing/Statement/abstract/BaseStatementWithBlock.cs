using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseStatementWithBlock : BaseStatement
{
    protected readonly KeywordToken Keyword;
    protected BracketToken? OpeningBracket;
    protected BracketToken? ClosingBracket;

    protected BaseStatementWithBlock(ParsingState state, KeywordToken keyword)
    {
        Keyword = keyword;
        state.Increment();

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