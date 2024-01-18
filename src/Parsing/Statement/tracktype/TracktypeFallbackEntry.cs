using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal readonly struct TracktypeFallbackEntry
{
    public readonly ValueToken? Identifier;
    public readonly ColonToken? Colon;
    private readonly BracketToken? _openingBracket;
    private readonly (ValueToken? identifier, BinaryOpToken? comma)[]? _fallback;
    private readonly BracketToken? _closingBracket;
    private readonly BinaryOpToken? _comma;

    public TracktypeFallbackEntry(ParsingState state, ValueToken? identifier, ColonToken colon)
    {
        Identifier = identifier;
        Colon = colon;

        List<(ValueToken? identifier, BinaryOpToken? comma)> fallbacks = new();
        ValueToken? current = null;

        var token = state.nextToken;
        for (; _closingBracket is null && token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                    goto label_Ending;

                case BinaryOpToken { Type: OperatorType.Comma } commaToken:
                    fallbacks.Add((current, commaToken));
                    current = null;
                    break;

                case BracketToken { Bracket: '[' } openingBracket when _openingBracket is null:
                    _openingBracket = openingBracket;
                    break;

                case BracketToken { Bracket: ']' } closingBracket:
                    _closingBracket = closingBracket;
                    break;

                case NumericToken:
                    goto default;

                case ValueToken valueToken when current is null:
                    current = valueToken;
                    break;

                case KeywordToken { Type: not KeywordType.Return, IsExpressionUsable: false }:
                    goto label_Ending;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        for (; _comma is null && token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                case KeywordToken { Type: not KeywordType.Return, IsExpressionUsable: false }:
                    goto label_Ending;

                case BinaryOpToken { Type: OperatorType.Comma } commaToken:
                    _comma = commaToken;
                    break;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        label_Ending:
        _fallback = fallbacks.ToMaybeArray();
    }
}