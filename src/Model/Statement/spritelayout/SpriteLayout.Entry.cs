using NMLServer.Lexing;

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

        private Entry(ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            state.Increment();
            _attributes = NMLAttribute.ParseSomeInBlock(state, ref _closingBracket);
        }

        private Entry(ParsingState state, IdentifierToken identifier)
        {
            _identifier = identifier;
            for (var token = state.nextToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        _openingBracket = openingBracket;
                        state.Increment();
                        _attributes = NMLAttribute.ParseSomeInBlock(state, ref _closingBracket);
                        return;

                    case BracketToken { Bracket: '}' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        return;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        }

        static List<Entry>? IAllowsParseInsideBlock<Entry>.ParseSomeInBlock(ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Entry> entries = new();
            for (var token = state.currentToken; token is not null; token = state.currentToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        entries.Add(new Entry(state, openingBracket));
                        break;

                    case BracketToken { Bracket: '}' } expectedClosingBracket:
                        closingBracket = expectedClosingBracket;
                        state.Increment();
                        return entries.ToMaybeList();

                    case IdentifierToken identifierToken:
                        entries.Add(new Entry(state, identifierToken));
                        break;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                        return entries.ToMaybeList();

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
            return entries;
        }
    }
}