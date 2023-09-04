using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed class NMLParser : BlockParser
{
    public static NMLFileRoot Apply()
    {
        var root = new NMLFileRoot();
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                // top-level does not allow for "param" block, so the only way it can be used is in the assignment
                case BracketToken { Bracket: not ('{' or '}') }:
                case BaseValueToken:
                case KeywordToken { IsExpressionUsable: true }:
                    var assignment = TryParseAssignment(root);
                    root.Children.Add(assignment);
                    if (assignment.Semicolon == null)
                    {
                        continue;
                    }
                    break;

                case KeywordToken { IsBlock: true } keywordToken:
                    var block = ParseBlockStatement(root, keywordToken);
                    root.Children.Add(block);
                    continue;

                case KeywordToken keywordToken:
                    root.Children.Add(ParseFunctionStatement(root, keywordToken));
                    break;

                default:
                    UnexpectedTokens.Add(current);
                    Pointer++;
                    break;
            }
            Pointer++;
        }
        return root;
    }

    private static Assignment TryParseAssignment(BaseStatementAST? parent)
    {
        TryParseExpression(out var leftHandSide, out var expectedEqualsSign);

        Assignment result;
        switch (expectedEqualsSign)
        {
            case AssignmentToken equalsSign:
                result = new Assignment(parent)
                {
                    LeftHandSide = leftHandSide,
                    EqualsSign = equalsSign,
                };
                goto afterAssignmentOperator;

            case null:
                return new Assignment(parent)
                {
                    LeftHandSide = leftHandSide
                };

            default:
                result = new Assignment(parent)
                {
                    LeftHandSide = leftHandSide
                };
                break;
        }
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken {Bracket: '{' or '}' }:
                case KeywordToken {IsExpressionUsable: false}:
                    return result;

                case AssignmentToken equalsSign:
                    result.EqualsSign = equalsSign;
                    break;

                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }

        afterAssignmentOperator:
        if (++Pointer >= Max)
        {
            return result;
        }
        TryParseExpression(out var righHandSide, out var expectedSemicolon);
        // Embrace cache locality
        result.RighHandSide = righHandSide;

        switch (expectedSemicolon)
        {
            case SemicolonToken semicolon:
                result.Semicolon = semicolon;
                return result;

            case null:
                return result;
        }

        while (Pointer < Max)
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