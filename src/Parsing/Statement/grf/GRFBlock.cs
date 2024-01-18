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

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    return;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        _attributes = attributes.ToMaybeArray();
        _parameters = parameters.ToMaybeArray();
    }
}