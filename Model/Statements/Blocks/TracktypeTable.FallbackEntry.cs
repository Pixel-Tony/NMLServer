using NMLServer.Extensions;
using NMLServer.Model.Grammar;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

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

        public int End => _comma?.End ?? _closingBracket?.End ?? _fallback?[^1].End ?? _openingBracket?.End
            ?? _colon?.End ?? _identifier.End;

        public FallbackEntry(ref ParsingState state, BaseValueToken identifier, ColonToken colon)
        {
            _identifier = identifier;
            _colon = colon;
            List<ValueWithComma<BaseValueToken>> fallbacks = [];
            BaseValueToken? current = null;
            while (state.NextToken is { } token)
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

                    case KeywordToken { IsDefiningStatement: true }:
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        label_ParsingComma:
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' }:
                    case KeywordToken { IsDefiningStatement: true }:
                        goto label_End;

                    case BinaryOpToken { Type: OperatorType.Comma } commaToken:
                        _comma = commaToken;
                        state.Increment();
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
        label_End:
            _fallback = fallbacks.ToMaybeList();
        }
    }
}