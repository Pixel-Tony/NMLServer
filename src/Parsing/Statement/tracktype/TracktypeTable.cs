using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class TracktypeTable : BaseStatementWithBlock
{
    private (ValueToken? identifier, BinaryOpToken? comma)[]? _entries;
    private TracktypeFallbackEntry[]? _fallbackEntries;

    public TracktypeTable(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<(ValueToken? identifier, BinaryOpToken? comma)> entries = new();
        List<TracktypeFallbackEntry> fallbacks = new();

        IdentifierToken? current = null;
        for (var token = state.currentToken; token is not null;)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_Ending;

                case BinaryOpToken { Type: OperatorType.Comma } commaToken:
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

                case KeywordToken { Type: not KeywordType.Return, IsExpressionUsable: false }:
                    goto label_Ending;

                default:
                    state.AddUnexpected(token);
                    break;
            }
            token = state.nextToken;
        }

        label_Ending:
        _entries = entries.ToMaybeArray();
        _fallbackEntries = fallbacks.ToMaybeArray();
    }
}