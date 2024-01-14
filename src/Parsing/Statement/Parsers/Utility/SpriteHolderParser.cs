using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statements.Models;

namespace NMLServer.Parsing.Statements;

internal class SpriteHolderParser : HeadingParser
{
    public static T Apply<T>(KeywordToken keyword) where T : SpriteHolder, new()
    {
        T result = new();
        ParseHeading(keyword, out result.Heading);

        while (result.OpeningBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    return result;

                case BracketToken { Bracket: '{' } opening:
                    result.OpeningBracket = opening;
                    break;

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

        result.Content = ExpressionSequenceParser.Apply();

        while (result.ClosingBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    break;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        return result;
    }
}