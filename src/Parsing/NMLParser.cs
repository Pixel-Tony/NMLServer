using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class NMLParser : BlockParser
{
    public static NMLFileRoot Apply()
    {
        var root = new NMLFileRoot();
        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            var statement = current switch
            {
                IdentifierToken
                        // top-level does not allow for "param" block, so the only way it can be used is in the assignment
                        or KeywordToken { Type: KeywordType.Param }
                        or BracketToken { Bracket: not ('{' or '}') }
                    => ParseAssignment(),

                KeywordToken { IsBlock: true } keyword
                    => ParseBlockStatement(keyword),

                KeywordToken keywordToken
                    => ParseFunctionStatement(keywordToken),

                _ => null
            };

            if (statement is null)
            {
                UnexpectedTokens.Add(current);
                Pointer++;
                continue;
            }
            root.Children.Add(statement);
        }
        return root;
    }

    private static Assignment ParseAssignment()
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
                case KeywordToken { IsExpressionUsable: false }:
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
                case KeywordToken { IsExpressionUsable: false }:
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