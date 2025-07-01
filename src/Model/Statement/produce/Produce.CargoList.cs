using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal partial class Produce
{
    private readonly struct CargoList
    {
        private readonly BracketToken? _openingBracket;
        private readonly List<NMLAttribute>? _content;
        private readonly BracketToken? _closingBracket;

        public int? end => _closingBracket?.end ?? _content?[^1].end ?? _openingBracket?.end;

        public CargoList(BracketToken closingBracket)
        {
            _closingBracket = closingBracket;
        }

        public CargoList(ref ParsingState state, BracketToken openingBracket)
        {
            List<NMLAttribute> content = [];
            _openingBracket = openingBracket;

            while (state.nextToken is { } token)
                switch (token)
                {
                    case BracketToken { Bracket: ']' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        goto label_End;
                    case ColonToken colonToken:
                        content.Add(new NMLAttribute(ref state, colonToken));
                        break;
                    case IdentifierToken identifierToken:
                        content.Add(new NMLAttribute(ref state, identifierToken));
                        break;
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                        goto label_End;
                    default:
                        state.AddUnexpected(token);
                        break;
                }

            label_End:
            _content = content.ToMaybeList();
        }
    }
}