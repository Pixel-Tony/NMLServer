using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed partial class SpriteLayout : BaseStatementWithBlock
{
    private readonly IReadOnlyList<Entry>? _entries;

    public SpriteLayout(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<Entry> entries = new();
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    entries.Add(new Entry(state, openingBracket));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    _entries = entries.ToMaybeList();
                    return;

                case IdentifierToken identifierToken:
                    entries.Add(new Entry(state, identifierToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    _entries = entries.ToMaybeList();
                    return;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        _entries = entries.ToMaybeList();
    }
}