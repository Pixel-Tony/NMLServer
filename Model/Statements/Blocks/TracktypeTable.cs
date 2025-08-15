using NMLServer.Extensions;
using NMLServer.Model.Grammar;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed partial class TracktypeTable : BaseBlockStatement
{
    private readonly List<ValueWithComma<BaseValueToken>>? _entries;
    private readonly List<FallbackEntry>? _fallbackEntries;

    protected override int? MiddleEnd => (_entries, _fallbackEntries).LastOf();

    public TracktypeTable(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is not null)
            return;
        List<ValueWithComma<BaseValueToken>> entries = [];
        List<FallbackEntry> fallbacks = [];
        IdentifierToken? current = null;
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                case BinaryOpToken { Type: OperatorType.Comma } commaToken when current is not null:
                    entries.Add(new ValueWithComma<BaseValueToken>(current, commaToken));
                    current = null;
                    break;

                case ColonToken colonToken when current is not null:
                    fallbacks.Add(new FallbackEntry(ref state, current, colonToken));
                    current = null;
                    continue;

                case IdentifierToken identifierToken when current is null:
                    current = identifierToken;
                    break;

                case KeywordToken { IsDefiningStatement: true }:
                    if (current is not null)
                        entries.Add(new ValueWithComma<BaseValueToken>(current, null));
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    break;
            }
            state.Increment();
        }
    label_End:
        _entries = entries.ToMaybeList();
        _fallbackEntries = fallbacks.ToMaybeList();
    }
}