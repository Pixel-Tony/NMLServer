using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class TracktypeTable : BlockStatement
{
    private readonly List<ValueWithComma<BaseValueToken>>? _entries;
    private readonly List<FallbackEntry>? _fallbackEntries;

    protected override int middleEnd => IHasEnd.LastOf(_entries, _fallbackEntries);

    public TracktypeTable(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword, ParamInfo.None)
    {
        if (ClosingBracket is not null)
            return;

        List<ValueWithComma<BaseValueToken>> entries = [];
        List<FallbackEntry> fallbacks = [];
        IdentifierToken? current = null;
        for (var token = state.currentToken; token is not null; token = state.currentToken)
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

                case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    if (current is not null)
                    {
                        entries.Add(new ValueWithComma<BaseValueToken>(current, null));
                    }
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