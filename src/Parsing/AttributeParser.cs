using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class AttributeParser : ExpressionParser
{
    protected static Pair<T, ExpressionAST> ParseAttribute<T>(T? start, ColonToken? colon) where T : Token
    {
        var result = new Pair<T, ExpressionAST>(start, colon);
        TryParseExpression(out result.Value, out var current);
        if (current is not SemicolonToken semicolonToken)
        {
            return result;
        }
        result.Semicolon = semicolonToken;
        Pointer++;
        return result;
    }

    protected static void ParseAttribute<T>(ref Pair<T, ExpressionAST> sketch) where T : Token
    {
        TryParseExpression(out sketch.Value, out var current);
        if (current is not SemicolonToken semicolon)
        {
            return;
        }
        sketch.Semicolon = semicolon;
        Pointer++;
    }

    protected static Pair<T, ExpressionAST> ParseAttribute<T>(T? start) where T : Token
    {
        var result = new Pair<T, ExpressionAST>(start);
        Pointer++;
        if (Pointer >= Max)
        {
            return result;
        }
        switch (Tokens[Pointer])
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
                throw new NotImplementedException();
        }
        Pointer++;
        if (Pointer >= Max)
        {
            return result;
        }

        TryParseExpression(out result.Value, out var current);
        if (current is not SemicolonToken semicolonToken)
        {
            return result;
        }
        result.Semicolon = semicolonToken;
        Pointer++;
        return result;
    }
}