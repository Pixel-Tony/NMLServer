using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing.Statement;

internal class BasecostParser : AttributeParser
{
    public static BaseStatement Apply(KeywordToken alwaysBasecost)
    {
        Basecost result = new(alwaysBasecost);
        List<NMLAttribute> attributes = new();

        Pointer++;
        while (result.OpeningBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' } opening:
                    result.OpeningBracket = opening;
                    break;

                case KeywordToken:
                    return result;

                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    Pointer++;
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        if (!areTokensLeft)
        {
            return result;
        }

        while (result.ClosingBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    result.ClosingBracket = closingBracket;
                    break;

                case KeywordToken:
                    goto end;

                case IdentifierToken identifier:
                    attributes.Add(ParseAttribute(identifier));
                    continue;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }

        end:
        result.Body = attributes.MaybeToArray();
        return result;
    }
}