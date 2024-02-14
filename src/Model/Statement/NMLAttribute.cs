using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal record struct NMLAttribute
{
    private readonly BaseMulticharToken? _key;
    private readonly ColonToken? _colon;
    private ExpressionAST? _value;
    private SemicolonToken? _semicolon;

    public NMLAttribute(BaseMulticharToken? key, ColonToken? colon, ExpressionAST? value, SemicolonToken? semicolon)
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
        state.IncrementSkippingComments();
        _value = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolon();
    }

    public NMLAttribute(ParsingState state, BaseMulticharToken key)
    {
        _key = key;
        for (var token = state.nextToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    _colon = colonToken;
                    state.IncrementSkippingComments();
                    _value = ExpressionAST.TryParse(state);
                    _semicolon = state.ExpectSemicolon();
                    return;

                case SemicolonToken semicolonToken:
                    _semicolon = semicolonToken;
                    state.Increment();
                    return;

                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }

    public static List<NMLAttribute>? TryParseSomeInBlock(ParsingState state, ref BracketToken? expectedClosingBracket)
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
                    return attributes.ToMaybeList();

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return attributes.ToMaybeList();
    }
}