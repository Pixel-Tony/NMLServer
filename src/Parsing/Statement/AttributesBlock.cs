using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal static class AttributesBlock
{
    public static NMLAttribute[]? TryParseManyInBlock(ParsingState state, ref BracketToken? expectedClosingBracket)
    {
        List<NMLAttribute> attributes = new();
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(state, colonToken));
                    break;

                case IdentifierToken identifierToken:
                    attributes.Add(new NMLAttribute(state, identifierToken));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    expectedClosingBracket = closingBracket;
                    state.Increment();
                    return attributes.ToMaybeArray();

                case KeywordToken { IsExpressionUsable: false }:
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return attributes.ToMaybeArray();
    }
}