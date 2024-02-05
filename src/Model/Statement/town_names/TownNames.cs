using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed partial class TownNames : BaseStatementWithBlock
{
    private readonly IReadOnlyList<NMLAttribute>? _attributes;
    private readonly IReadOnlyList<Part>? _parts;

    public TownNames(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<NMLAttribute> attributes = new();
        List<Part> parts = new();
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingInnerBracket:
                    parts.Add(new Part(state, openingInnerBracket));
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