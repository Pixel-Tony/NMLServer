using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing;

internal record struct NMLAttribute
{
    private readonly MulticharToken? _key;
    private readonly ColonToken? _colon;
    private ExpressionAST? _value;
    private SemicolonToken? _semicolon;

    public NMLAttribute(MulticharToken? key, ColonToken? colon, ExpressionAST? value, SemicolonToken? semicolon)
    {
        _key = key;
        _colon = colon;
        _value = value;
        _semicolon = semicolon;
    }

    public NMLAttribute(ParsingState state, ColonToken colon)
    {
        _key = null;
        _colon = colon;

        state.Increment();
        _value = ExpressionAST.TryParse(state);

        _semicolon = state.ExpectSemicolonAfterExpression();
    }

    public NMLAttribute(ParsingState state, MulticharToken key)
    {
        _key = key;

        for (var token = state.nextToken; token is not null && _colon is null; token = state.nextToken)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    _colon = colonToken;
                    break;

                case SemicolonToken semicolonToken:
                    _semicolon = semicolonToken;
                    state.Increment();
                    return;

                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { IsExpressionUsable: false }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }

        _value = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolonAfterExpression();
    }

    public static NMLAttribute[]? TryParseManyInBlock(ParsingState state, ref BracketToken? expectedClosingBracket)
    {
        List<NMLAttribute> attributes = new();
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(state, colonToken));
                    break;

                case IdentifierToken identifierToken:
                    attributes.Add(new NMLAttribute(state, identifierToken));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    expectedClosingBracket = closingBracket;
                    state.Increment();
                    return attributes.ToMaybeArray();

                case KeywordToken { IsExpressionUsable: false }:
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return attributes.ToMaybeArray();
    }
}