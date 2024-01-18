using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal readonly record struct GRFParameterNames
{
    private readonly IdentifierToken? _identifier;
    private readonly ColonToken? _colon;
    private readonly BracketToken _openingBracket;
    private readonly NMLAttribute[]? _items;
    private readonly BracketToken? _closingBracket;
    private readonly SemicolonToken? _semicolon;

    public GRFParameterNames(ParsingState state, IdentifierToken? identifier, ColonToken? colon,
        BracketToken openingBracket)
    {
        _identifier = identifier;
        _colon = colon;
        _openingBracket = openingBracket;

        List<NMLAttribute> attributes = new();
        for (var token = state.nextToken; _closingBracket is null && token is not null;)
        {
            switch (token)
            {
                case NumericToken numericToken:
                    attributes.Add(new NMLAttribute(state, numericToken));
                    token = state.currentToken;
                    continue;

                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(state, colonToken));
                    token = state.currentToken;
                    continue;

                case BracketToken { Bracket: '}' } closingBracket:
                    _closingBracket = closingBracket;
                    break;

                case KeywordToken:
                    goto label_Ending;

                default:
                    state.AddUnexpected(token);
                    break;
            }
            token = state.nextToken;
        }
        _semicolon = state.ExpectSemicolonAfterExpression();
        label_Ending:
        _items = attributes.ToMaybeArray();
    }
}