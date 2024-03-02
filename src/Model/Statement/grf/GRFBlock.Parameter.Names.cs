using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal partial class GRFBlock
{
    private partial class Parameter
    {
        private readonly struct Names : IHasEnd
        {
            private readonly IdentifierToken? _name;
            private readonly ColonToken? _colon;
            private readonly BracketToken _openingBracket;
            private readonly IReadOnlyList<NMLAttribute>? _items;
            private readonly BracketToken? _closingBracket;
            private readonly SemicolonToken? _semicolon;

            public int end => _semicolon?.end ?? (_closingBracket?.end ?? (_items?[^1].end ?? (_openingBracket?.end
                ?? (_colon?.end ?? _name!.end))));

            public Names(ParsingState state, IdentifierToken? name, ColonToken? colon,
                BracketToken openingBracket)
            {
                _name = name;
                _colon = colon;
                _openingBracket = openingBracket;
                List<NMLAttribute> attributes = [];
                for (var token = state.nextToken; token is not null; token = state.currentToken)
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

                        case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                            _items = attributes.ToMaybeList();
                            return;

                        default:
                            state.AddUnexpected(token);
                            state.Increment();
                            break;
                    }
                }
                _items = attributes.ToMaybeList();
            }
        }
    }
}