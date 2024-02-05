using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed partial class SpriteLayout
{
    private readonly record struct Entry
    {
        private readonly IdentifierToken? _identifier;
        private readonly BracketToken? _openingBracket;
        private readonly IReadOnlyList<NMLAttribute>? _attributes;
        private readonly BracketToken? _closingBracket;

        public Entry(ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            state.Increment();
            _attributes = NMLAttribute.TryParseSomeInBlock(state, ref _closingBracket);
        }

        public Entry(ParsingState state, IdentifierToken identifier)
        {
            _identifier = identifier;
            for (var token = state.nextToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        _openingBracket = openingBracket;
                        state.Increment();
                        _attributes = NMLAttribute.TryParseSomeInBlock(state, ref _closingBracket);
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
    }
}