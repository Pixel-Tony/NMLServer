using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class TileLayout : BlockStatement
{
    private readonly List<NMLAttribute>? _attributes;
    private readonly List<Entry>? _entries;

    protected override int? MiddleEnd => IHasEnd.LastOf(_attributes, _entries);

    public TileLayout(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword,
        new ParamInfo(false, (1, 1), (0, SymbolKind.Variable)))
    {
        if (ClosingBracket is not null)
            return;

        List<NMLAttribute> attributes = [];
        List<Entry> entries = [];
        while (state.CurrentToken is { } token)
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

                case KeywordToken { Kind: KeywordKind.BlockDefining }:
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