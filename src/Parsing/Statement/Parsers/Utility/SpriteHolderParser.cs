using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing.Statement;

internal class SpriteHolderParser : ExpressionParser
{
    public static T Apply<T>(KeywordToken keyword) where T : BaseSpriteHolder, new()
    {
        T result = new();
        HeadingParser.Apply(keyword, out result.Heading);

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

                case KeywordToken { IsExpressionUsable: false }:
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