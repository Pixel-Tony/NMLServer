using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing.Statement;

internal class SpriteHolderParser : ExpressionParser
{
    public static void Apply(KeywordToken keyword, BaseSpriteHolder result)
    {
        HeadingParser.Apply(keyword, out result.Heading);

        while (result.OpeningBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    return;

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
            return;
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
                    return;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
    }
}