using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal partial class TracktypeTable
{
    private readonly record struct FallbackEntry
    {
        public readonly BaseValueToken? Identifier;
        public readonly ColonToken? Colon;
        private readonly BracketToken? _openingBracket;
        private readonly IReadOnlyList<(BaseValueToken? identifier, BinaryOpToken? comma)>? _fallback;
        private readonly BracketToken? _closingBracket;
        private readonly BinaryOpToken? _comma;

        public FallbackEntry(ParsingState state, BaseValueToken? identifier, ColonToken colon)
        {
            Identifier = identifier;
            Colon = colon;
            List<(BaseValueToken? identifier, BinaryOpToken? comma)> fallbacks = new();
            BaseValueToken? current = null;
            for (var token = state.nextToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' }:
                        _fallback = fallbacks.ToMaybeList();
                        return;

                    case BinaryOpToken { Type: OperatorType.Comma } commaToken:
                        fallbacks.Add((current, commaToken));
                        current = null;
                        break;

                    case BracketToken { Bracket: '[' } openingBracket when _openingBracket is null:
                        _openingBracket = openingBracket;
                        break;

                    case BracketToken { Bracket: ']' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        goto label_ParsingComma;

                    case NumericToken:
                        goto default;

                    case BaseValueToken valueToken when current is null:
                        current = valueToken;
                        break;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                        _fallback = fallbacks.ToMaybeList();
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            label_ParsingComma:
            for (var token = state.currentToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' }:
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                        _fallback = fallbacks.ToMaybeList();
                        return;

                    case BinaryOpToken { Type: OperatorType.Comma } commaToken:
                        _comma = commaToken;
                        state.Increment();
                        _fallback = fallbacks.ToMaybeList();
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            _fallback = fallbacks.ToMaybeList();
        }
    }
}