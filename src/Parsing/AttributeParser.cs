using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class AttributeParser : ExpressionParser
{
    protected static Pair<T, ExpressionAST> ParseAttribute<T>(T? start) where T : Token
    {
        var result = new Pair<T, ExpressionAST>(start);
        Pointer++;
        if (!areTokensLeft)
        {
            return result;
        }
        while (result.Colon is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case ColonToken colonToken:
                    result.Colon = colonToken;
                    break;

                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
                    return result;

                case BracketToken { Bracket: '{' or '}' }:
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
        TryParseExpression(out result.Value, out var expressionEnder);
        if (expressionEnder is not SemicolonToken semicolonToken)
        {
            return result;
        }
        result.Semicolon = semicolonToken;
        Pointer++;
        return result;
    }
}