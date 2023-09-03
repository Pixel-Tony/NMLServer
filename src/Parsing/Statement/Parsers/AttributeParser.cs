using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class AttributeParser : BaseParser
{
    public static Pair<T, ExpressionAST> Apply<T>(T start, ColonToken colon) where T : Token
    {
        Pair<T, ExpressionAST> result = new(start, colon);
        if (++Pointer >= Max)
        {
            return result;
        }
        var (expr, current) = ExpressionParser.Apply();
        result.Value = expr;
        if (current is SemicolonToken semicolonToken)
        {
            result.Semicolon = semicolonToken;
        }
        return result;
    }

    public static Pair<T, ExpressionAST> Apply<T>(T start) where T : Token
    {
        Pair<T, ExpressionAST> result = new(start);
        Pointer++;
        if (Pointer >= Max)
        {
            return result;
        }
        switch (Tokens[Pointer])
        {
            // Default assignment
            case ColonToken colonToken:
                result.Colon = colonToken;
                break;
            // Parameter block, after parameter name
            case BracketToken { Bracket: '{' }:
                return result;
            default:
                // TODO:
                throw new Exception();
        }
        Pointer++;
        if (Pointer >= Max)
        {
            return result;
        }
        var (expr, current) = ExpressionParser.Apply();
        result.Value = expr;
        if (current is SemicolonToken semicolonToken)
        {
            result.Semicolon = semicolonToken;
        }
        return result;
    }
}