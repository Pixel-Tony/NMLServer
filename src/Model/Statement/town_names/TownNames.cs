using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed partial class TownNames : StatementWithBlock
{
    private readonly List<NMLAttribute>? _attributes;
    private readonly List<Part>? _parts;

    protected override int middleEnd => Extensions.LastOf(_attributes, _parts);

    public TownNames(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<NMLAttribute> attributes = [];
        List<Part> parts = [];
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingInnerBracket:
                    parts.Add(new Part(ref state, openingInnerBracket));
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

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        label_End:
        _attributes = attributes.ToMaybeList();
        _parts = parts.ToMaybeList();
    }
}