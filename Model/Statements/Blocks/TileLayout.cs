using NMLServer.Extensions;
using NMLServer.Model.Grammar;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed partial class TileLayout : BaseBlockStatement
{
    private readonly List<PropertySetting>? _attributes;
    private readonly List<Entry>? _entries;

    protected override int? MiddleEnd => (_attributes, _entries).LastOf();

    public TileLayout(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is not null)
            return;

        List<PropertySetting> attributes = [];
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
                    attributes.Add(new PropertySetting(ref state, identifier));
                    break;

                case KeywordToken { IsDefiningStatement: true }:
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