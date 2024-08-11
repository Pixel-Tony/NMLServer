using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal sealed partial class GRFBlock : StatementWithBlock
{
    private readonly List<NMLAttribute>? _attributes;
    private readonly List<Parameter>? _parameters;

    protected override int middleEnd => Extensions.LastOf(_attributes, _parameters);

    public GRFBlock(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<NMLAttribute> attributes = [];
        List<Parameter> parameters = [];

        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case IdentifierToken identifierToken:
                    attributes.Add(new NMLAttribute(ref state, identifierToken));
                    break;

                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(ref state, colonToken));
                    break;

                case KeywordToken { Type: KeywordType.Param } paramToken:
                    parameters.Add(new Parameter(ref state, paramToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        label_End:
        _attributes = attributes.ToMaybeList();
        _parameters = parameters.ToMaybeList();
    }
}