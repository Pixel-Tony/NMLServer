using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class TownNames : BlockStatement
{
    private readonly List<NMLAttribute>? _attributes;
    private readonly List<Part>? _parts;

    protected override int MiddleEnd => IHasEnd.LastOf(_attributes, _parts);

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        // TODO parameter syntax requires additional handling
    }

    public TownNames(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword,
        new ParamInfo(false, (0, 1)))
    {
        if (ClosingBracket is not null)
            return;

        List<NMLAttribute> attributes = [];
        List<Part> parts = [];
        while (state.CurrentToken is { } token)
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

                case KeywordToken { Kind: KeywordKind.BlockDefining }:
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