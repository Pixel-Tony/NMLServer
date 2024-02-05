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
        state.IncrementSkippingComments();
        Parameters = ExpressionAST.TryParse(state);
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    OpeningBracket = openingBracket;
                    state.IncrementSkippingComments();
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.IncrementSkippingComments();
                    return;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }
}