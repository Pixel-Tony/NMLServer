using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal partial class GRFBlock
{
    private partial class Parameter
    {
        private readonly record struct Names
        {
            private readonly IdentifierToken? _name;
            private readonly ColonToken? _colon;
            private readonly BracketToken _openingBracket;
            private readonly IReadOnlyList<NMLAttribute>? _items;
            private readonly BracketToken? _closingBracket;
            private readonly SemicolonToken? _semicolon;

            public Names(ParsingState state, IdentifierToken? name, ColonToken? colon,
                BracketToken openingBracket)
            {
                _name = name;
                _colon = colon;
                _openingBracket = openingBracket;
                List<NMLAttribute> attributes = new();
                for (var token = state.nextToken;
                     _closingBracket is null && token is not null;
                     token = state.currentToken)
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
                            _items = attributes.ToMaybeList();
                            state.Increment();
                            _semicolon = state.ExpectSemicolon();
                            return;

                        case KeywordToken:
                            _items = attributes.ToMaybeList();
                            return;

                        default:
                            state.AddUnexpected(token);
                            state.Increment();
                            break;
                    }
                }
                _items = attributes.ToMaybeList();
                _semicolon = state.ExpectSemicolon();
            }
        }
    }
}