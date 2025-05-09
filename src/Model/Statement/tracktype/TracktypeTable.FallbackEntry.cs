using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal partial class TracktypeTable
{
    private readonly struct FallbackEntry : IHasEnd
    {
        private readonly BaseValueToken _identifier;
        private readonly ColonToken? _colon;
        private readonly BracketToken? _openingBracket;
        private readonly List<ValueWithComma<BaseValueToken>>? _fallback;
        private readonly BracketToken? _closingBracket;
        private readonly BinaryOpToken? _comma;

        public int end => _comma?.end ?? _closingBracket?.end ?? _fallback?[^1].end ?? _openingBracket?.end
            ?? _colon?.end ?? _identifier.end;

        public FallbackEntry(ref ParsingState state, BaseValueToken identifier, ColonToken colon)
        {
            _identifier = identifier;
            _colon = colon;
            List<ValueWithComma<BaseValueToken>> fallbacks = [];
            BaseValueToken? current = null;
            for (var token = state.nextToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' }:
                        goto label_End;

                    case BinaryOpToken { Type: OperatorType.Comma } commaToken when current is not null:
                        fallbacks.Add(new ValueWithComma<BaseValueToken>(current, commaToken));
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

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                        goto label_End;

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
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                        goto label_End;

                    case BinaryOpToken { Type: OperatorType.Comma } commaToken:
                        _comma = commaToken;
                        state.Increment();
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            label_End:
            _fallback = fallbacks.ToMaybeList();
        }
    }
}