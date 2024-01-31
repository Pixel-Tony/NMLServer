using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class TracktypeTable : BaseStatementWithBlock
{
    private (BaseValueToken? identifier, BinaryOpToken? comma)[]? _entries;
    private TracktypeFallbackEntry[]? _fallbackEntries;

    public TracktypeTable(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<(BaseValueToken? identifier, BinaryOpToken? comma)> entries = new();
        List<TracktypeFallbackEntry> fallbacks = new();

        IdentifierToken? current = null;
        for (var token = state.currentToken; token is not null;)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                case BinaryOpToken { Type: OperatorType.Comma } commaToken when current is not null:
                    entries.Add((current, commaToken));
                    current = null;
                    break;

                case ColonToken colonToken:
                    fallbacks.Add(new TracktypeFallbackEntry(state, current, colonToken));
                    current = null;
                    token = state.currentToken;
                    continue;

                case IdentifierToken identifierToken when current is null:
                    current = identifierToken;
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    break;
            }
            token = state.nextToken;
        }
        label_End:
        _entries = entries.ToArrayOrNull();
        _fallbackEntries = fallbacks.ToArrayOrNull();
    }
}