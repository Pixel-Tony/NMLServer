using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing.Statement;

internal class AssignmentParser : ExpressionParser
{
    public static Assignment Apply()
    {
        TryParseExpression(out var leftHandSide, out var expectedEqualsSign);

        Assignment result;
        switch (expectedEqualsSign)
        {
            case null:
                return new Assignment(leftHandSide);

            case AssignmentToken equalsSign:
                result = new Assignment(leftHandSide, equalsSign);
                Pointer++;
                break;

            default:
                result = new Assignment(leftHandSide);
                break;
        }

        while (result.EqualsSign is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return result;

                case AssignmentToken equalsSign:
                    result.EqualsSign = equalsSign;
                    break;

                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
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
        TryParseExpression(out result.RighHandSide, out var expectedSemicolon);
        switch (expectedSemicolon)
        {
            case SemicolonToken semicolon:
                result.Semicolon = semicolon;
                Pointer++;
                return result;

            case null:
                return result;
        }

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
                    Pointer++;
                    return result;

                case null:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                case BracketToken { Bracket: '{' or '}' }:
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