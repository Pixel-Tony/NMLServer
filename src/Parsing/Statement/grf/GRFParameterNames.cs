using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal readonly record struct GRFParameterNames
{
    private readonly IdentifierToken? _name;
    private readonly ColonToken? _colon;
    private readonly BracketToken _openingBracket;
    private readonly NMLAttribute[]? _items;
    private readonly BracketToken? _closingBracket;
    private readonly SemicolonToken? _semicolon;

    public GRFParameterNames(ParsingState state, IdentifierToken? name, ColonToken? colon, BracketToken openingBracket)
    {
        _name = name;
        _colon = colon;
        _openingBracket = openingBracket;
        List<NMLAttribute> attributes = new();
        for (var token = state.nextToken; _closingBracket is null && token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case NumericToken numericToken:
                    attributes.Add(new NMLAttribute(state, numericToken));
                    continue;

                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(state, colonToken));
                    continue;

                case BracketToken { Bracket: '}' } closingBracket:
                    _closingBracket = closingBracket;
                    _items = attributes.ToArrayOrNull();
                    state.Increment();
                    _semicolon = state.ExpectSemicolon();
                    return;

                case KeywordToken:
                    _items = attributes.ToArrayOrNull();
                    return;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        _items = attributes.ToArrayOrNull();
        _semicolon = state.ExpectSemicolon();
    }
}