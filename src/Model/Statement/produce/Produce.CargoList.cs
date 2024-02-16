using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal partial class Produce
{
    private readonly record struct CargoList
    {
        private readonly BracketToken? _openingBracket;
        private readonly IReadOnlyList<NMLAttribute>? _content;
        private readonly BracketToken? _closingBracket;

        public int? end => _closingBracket?.end ?? (_content?[^1].end ?? _openingBracket?.end);

        public CargoList(BracketToken closingBracket)
        {
            _closingBracket = closingBracket;
        }

        public CargoList(ParsingState state, BracketToken openingBracket)
        {
            List<NMLAttribute> content = new();
            _openingBracket = openingBracket;

            for (var token = state.nextToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: ']' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        _content = content.ToMaybeList();
                        return;

                    case ColonToken colonToken:
                        content.Add(new NMLAttribute(state, colonToken));
                        break;

                    case IdentifierToken identifierToken:
                        content.Add(new NMLAttribute(state, identifierToken));
                        break;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                        _content = content.ToMaybeList();
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            _content = content.ToMaybeList();
        }
    }
}