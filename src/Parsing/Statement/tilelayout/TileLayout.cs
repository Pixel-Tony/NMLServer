using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class TileLayout : BaseStatementWithBlock
{
    private readonly NMLAttribute[]? _attributes;
    private readonly TileEntry[]? _entries;

    public TileLayout(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<NMLAttribute> attributes = new();
        List<TileEntry> entries = new();
        for (var token = state.nextToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } comma:
                    entries.Add(new TileEntry(state, null, comma));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();

                    _attributes = attributes.ToMaybeArray();
                    _entries = entries.ToMaybeArray();
                    return;

                case IdentifierToken identifier:
                    attributes.Add(new NMLAttribute(state, identifier));
                    break;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    _attributes = attributes.ToMaybeArray();
                    _entries = entries.ToMaybeArray();
                    return;

                case NumericToken numericToken:
                    entries.Add(new TileEntry(state, numericToken));
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        _attributes = attributes.ToMaybeArray();
        _entries = entries.ToMaybeArray();
    }
}