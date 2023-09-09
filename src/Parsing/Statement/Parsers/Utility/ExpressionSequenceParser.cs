using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing;

internal class ExpressionSequenceParser : ExpressionParser
{
    public static ExpressionAST[]? Apply(int startingListSize = 4)
    {
        List<ExpressionAST> result = new(startingListSize);
        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' }:
                    goto default;

                case BracketToken { Bracket: '}' }:
                case KeywordToken { IsExpressionUsable: false }:
                    return result.Count > 0
                        ? result.ToArray()
                        : null;

                case BracketToken:
                case KeywordToken:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    TryParseExpression(out var expression, out _);
                    if (expression is not null)
                    {
                        result.Add(expression);
                    }
                    continue;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        return result.Count > 0
            ? result.ToArray()
            : null;
    }
}