using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed partial class TileLayout : StatementWithBlock
{
    private readonly List<NMLAttribute>? _attributes;
    private readonly List<Entry>? _entries;

    protected override int middleEnd => Extensions.LastOf(_attributes, _entries);

    public TileLayout(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<NMLAttribute> attributes = [];
        List<Entry> entries = [];
        for (var token = state.nextToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } comma:
                    entries.Add(new Entry(ref state, null, comma));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                case IdentifierToken identifier:
                    attributes.Add(new NMLAttribute(ref state, identifier));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

                case NumericToken numericToken:
                    entries.Add(new Entry(ref state, numericToken));
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