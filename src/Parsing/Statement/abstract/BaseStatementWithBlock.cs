using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseStatementWithBlock : BaseStatement
{
    protected readonly KeywordToken Keyword;
    protected readonly ExpressionAST? Parameters;
    protected BracketToken? OpeningBracket;
    protected BracketToken? ClosingBracket;

    protected BaseStatementWithBlock(ParsingState state, KeywordToken keyword)
    {
        Keyword = keyword;
        state.Increment();
        Parameters = ExpressionAST.TryParse(state);
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