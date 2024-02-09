using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed partial class TileLayout : BaseStatementWithBlock
{
    private readonly IReadOnlyList<NMLAttribute>? _attributes;
    private readonly IReadOnlyList<Entry>? _entries;

    public TileLayout(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<NMLAttribute> attributes = new();
        List<Entry> entries = new();
        for (var token = state.nextToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } comma:
                    entries.Add(new Entry(state, null, comma));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                case IdentifierToken identifier:
                    attributes.Add(new NMLAttribute(state, identifier));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

                case NumericToken numericToken:
                    entries.Add(new Entry(state, numericToken));
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        label_End:
        _attributes = attributes.ToMaybeList();
        _entries = entries.ToMaybeList();
    }
}