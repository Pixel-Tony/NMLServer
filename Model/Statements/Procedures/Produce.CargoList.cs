using NMLServer.Extensions;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Procedures;

internal partial class Produce
{
    private readonly struct CargoList
    {
        private readonly BracketToken? _openingBracket;
        private readonly List<PropertySetting>? _content;
        private readonly BracketToken? _closingBracket;

        public int? End => _closingBracket?.End ?? _content?[^1].End ?? _openingBracket?.End;

        public CargoList(BracketToken closingBracket)
        {
            _closingBracket = closingBracket;
        }

        public CargoList(ref ParsingState state, BracketToken openingBracket)
        {
            List<PropertySetting> content = [];
            _openingBracket = openingBracket;

            while (state.NextToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: ']' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        goto label_End;

                    case ColonToken colonToken:
                        content.Add(new PropertySetting(ref state, colonToken));
                        break;

                    case IdentifierToken identifierToken:
                        content.Add(new PropertySetting(ref state, identifierToken));
                        break;

                    case KeywordToken { IsDefiningStatement: true }:
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        label_End:
            _content = content.ToMaybeList();
        }
    }
}