using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class SpriteLayout : BaseStatementWithBlock
{
    private readonly SpriteLayoutEntry[]? _entries;

    public SpriteLayout(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<SpriteLayoutEntry> entries = new();
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    entries.Add(new SpriteLayoutEntry(state, openingBracket));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    _entries = entries.ToArrayOrNull();
                    return;

                case IdentifierToken identifierToken:
                    entries.Add(new SpriteLayoutEntry(state, identifierToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    _entries = entries.ToArrayOrNull();
                    return;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        _entries = entries.ToArrayOrNull();
    }
}