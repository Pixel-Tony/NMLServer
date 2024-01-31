using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal readonly record struct TracktypeFallbackEntry
{
    public readonly BaseValueToken? Identifier;
    public readonly ColonToken? Colon;
    private readonly BracketToken? _openingBracket;
    private readonly (BaseValueToken? identifier, BinaryOpToken? comma)[]? _fallback;
    private readonly BracketToken? _closingBracket;
    private readonly BinaryOpToken? _comma;

    public TracktypeFallbackEntry(ParsingState state, BaseValueToken? identifier, ColonToken colon)
    {
        Identifier = identifier;
        Colon = colon;

        List<(BaseValueToken? identifier, BinaryOpToken? comma)> fallbacks = new();
        BaseValueToken? current = null;

        var token = state.nextToken;
        for (; _closingBracket is null && token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                    goto label_End;

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

                case BaseValueToken valueToken when current is null:
                    current = valueToken;
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

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
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

                case BinaryOpToken { Type: OperatorType.Comma } commaToken:
                    _comma = commaToken;
                    break;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        label_End:
        _fallback = fallbacks.ToArrayOrNull();
    }
}