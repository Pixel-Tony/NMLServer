using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal partial class GRFBlock
{
    private partial class Parameter
    {
        public partial struct Block
        {
            private readonly struct Names : IHasEnd
            {
                private readonly IdentifierToken? _name;
                private readonly ColonToken? _colon;
                private readonly BracketToken _openingBracket;
                private readonly List<NMLAttribute>? _items;
                private readonly BracketToken? _closingBracket;
                private readonly SemicolonToken? _semicolon;

                public int End => _semicolon?.End ?? _closingBracket?.End
                    ?? _items?[^1].End ?? _openingBracket?.End ?? _colon?.End ?? _name!.End;

                public Names(ref ParsingState state, IdentifierToken? name, ColonToken? colon,
                    BracketToken openingBracket)
                {
                    _name = name;
                    _colon = colon;
                    _openingBracket = openingBracket;
                    state.Increment();
                    List<NMLAttribute> attributes = [];
                    while (state.CurrentToken is { } token)
                    {
                        switch (token)
                        {
                            case NumericToken numericToken:
                                attributes.Add(new NMLAttribute(ref state, numericToken));
                                continue;

                            case ColonToken colonToken:
                                attributes.Add(new NMLAttribute(ref state, colonToken));
                                continue;

                            case BracketToken { Bracket: '}' } closingBracket:
                                _closingBracket = closingBracket;
                                state.Increment();
                                _semicolon = state.ExpectSemicolon();
                                goto label_End;

                            case KeywordToken { Kind: KeywordKind.BlockDefining }:
                                goto label_End;

                            default:
                                state.AddUnexpected(token);
                                state.Increment();
                                break;
                        }
                    }
                label_End:
                    _items = attributes.ToMaybeList();
                }
            }
        }
    }
}