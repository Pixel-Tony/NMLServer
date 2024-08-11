using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal readonly struct NMLAttribute : IAllowsParseInsideBlock<NMLAttribute>
{
    private readonly BaseMulticharToken? _key;
    private readonly ColonToken? _colon;
    private readonly ExpressionAST? _value;
    private readonly SemicolonToken? _semicolon;

    public int end => _semicolon?.end ?? (_value?.end ?? (_colon?.end ?? _key!.end));

    public NMLAttribute(BaseMulticharToken? key, ColonToken? colon, ExpressionAST? value, SemicolonToken? semicolon)
    {
        _key = key;
        _colon = colon;
        _value = value;
        _semicolon = semicolon;
    }

    public NMLAttribute(ref ParsingState state, ColonToken colon)
    {
        _key = null;
        _colon = colon;
        state.IncrementSkippingComments();
        _value = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }

    public NMLAttribute(ref ParsingState state, BaseMulticharToken key)
    {
        _key = key;
        for (var token = state.nextToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    _colon = colonToken;
                    state.IncrementSkippingComments();
                    _value = ExpressionAST.TryParse(ref state);
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

    public static List<NMLAttribute>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
    {
        List<NMLAttribute> attributes = [];
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(ref state, colonToken));
                    break;

                case IdentifierToken identifierToken:
                    attributes.Add(new NMLAttribute(ref state, identifierToken));
                    break;

                case BracketToken { Bracket: '}' } expectedClosingBracket:
                    closingBracket = expectedClosingBracket;
                    state.Increment();
                    goto label_End;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        label_End:
        return attributes.ToMaybeList();
    }
}