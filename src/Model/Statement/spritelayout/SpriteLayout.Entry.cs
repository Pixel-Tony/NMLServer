using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class SpriteLayout
{
    public readonly struct Entry : IAllowsParseInsideBlock<Entry>
    {
        private readonly IdentifierToken? _identifier;
        private readonly BracketToken? _openingBracket;
        private readonly IReadOnlyList<NMLAttribute>? _attributes;
        private readonly BracketToken? _closingBracket;

        public int end => _closingBracket?.end ?? (_attributes?[^1].end ?? (_openingBracket?.end ?? _identifier!.end));

        private Entry(ref ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            state.Increment();
            _attributes = NMLAttribute.ParseSomeInBlock(ref state, ref _closingBracket);
        }

        private Entry(ref ParsingState state, IdentifierToken identifier)
        {
            _identifier = identifier;
            for (var token = state.nextToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        _openingBracket = openingBracket;
                        state.Increment();
                        _attributes = NMLAttribute.ParseSomeInBlock(ref state, ref _closingBracket);
                        return;

                    case BracketToken { Bracket: '}' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        return;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        }

        static List<Entry>? IAllowsParseInsideBlock<Entry>.ParseSomeInBlock(ref ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Entry> entries = [];
            for (var token = state.currentToken; token is not null; token = state.currentToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        entries.Add(new Entry(ref state, openingBracket));
                        break;

                    case BracketToken { Bracket: '}' } expectedClosingBracket:
                        closingBracket = expectedClosingBracket;
                        state.Increment();
                        goto label_End;

                    case IdentifierToken identifierToken:
                        entries.Add(new Entry(ref state, identifierToken));
                        break;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
            label_End:
            return entries.ToMaybeList();
        }
    }
}