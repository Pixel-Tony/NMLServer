using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class GRFBlock : BaseStatementWithBlock
{
    private NMLAttribute[]? _attributes;
    private GRFParameter[]? _parameters;

    public GRFBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }

        List<NMLAttribute> attributes = new();
        List<GRFParameter> parameters = new();
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case IdentifierToken identifierToken:
                    attributes.Add(new NMLAttribute(state, identifierToken));
                    break;

                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(state, colonToken));
                    break;

                case KeywordToken { Type: KeywordType.Param } paramToken:
                    parameters.Add(new GRFParameter(state, paramToken));
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
        _attributes = attributes.ToArrayOrNull();
        _parameters = parameters.ToArrayOrNull();
    }
}