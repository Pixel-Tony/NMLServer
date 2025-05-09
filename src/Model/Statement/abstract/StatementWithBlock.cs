using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal abstract class StatementWithBlock : StatementAST
{
    private readonly KeywordToken _keyword;
    protected readonly ExpressionAST? Parameters;
    protected readonly BracketToken? OpeningBracket;
    public BracketToken? ClosingBracket;

    public sealed override int start => _keyword.start;

    public sealed override int end
    {
        get
        {
            if (ClosingBracket is not null)
            {
                return ClosingBracket.end;
            }
            int middle = middleEnd;
            return middle > 0
                ? middle
                : OpeningBracket?.end ?? (Parameters?.end ?? _keyword.end);
        }
    }

    protected abstract int middleEnd { get; }

    protected StatementWithBlock(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.IncrementSkippingComments();
        Parameters = ExpressionAST.TryParse(ref state);
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

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }
}