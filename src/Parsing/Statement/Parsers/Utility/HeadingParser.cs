using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing;

internal class HeadingParser : AttributeParser
{
    protected static void ParseHeading(KeywordToken keyword, out StatementHeading result)
    {
        result = new StatementHeading(keyword);
        Pointer++;

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                case SemicolonToken:
                    return;

                case BracketToken:
                case KeywordToken { Type: not KeywordType.Return }:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    TryParseExpression(out result.Parameters, out _);
                    return;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
    }
}