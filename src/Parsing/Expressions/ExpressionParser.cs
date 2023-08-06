using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class ExpressionParser
{
    public (ExpressionAST?, Token?) Parse(Token[] tokens, int start)
    {
        int i = start;
        int max = tokens.Length;

        ExpressionAST? tree = null;
        
        while (i < max)
        {
            var current = tokens[i];
            switch (current)
            {
                case BinaryOpToken binaryOpToken:
                    while (true)
                    {
                        switch (tree)
                        {
                            case null:
                                break;
                            case ValueNode valueNode:
                                break;
                            case Variable variable:
                                break;
                            case ArrayExpression arrayExpression:
                                break;
                            case BinaryOperation binaryOperation:
                                break;
                            case FunctionCall functionCall:
                                break;
                            case LiteralString literalString:
                                break;
                            case Number number:
                                break;
                            case ParentedExpression parentedExpression:
                                break;
                            case TernaryOperation ternaryOperation:
                                break;
                            case UnaryOperation unaryOperation:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(tree));
                        }
                    }
                    throw new NotImplementedException();
                case BracketToken bracketToken:
                    switch (bracketToken.Bracket)
                    {
                        case '(':
                            throw new NotImplementedException();
                        case '[':
                            throw new NotImplementedException();
                        // Any other bracket will finish the expression
                        default:
                            return (tree, current);
                    }
                case KeywordToken { Type: "var" }:
                    throw new NotImplementedException();
                case LiteralToken:
                    throw new NotImplementedException();
                case NumericToken:
                    throw new NotImplementedException();
                case TernaryOpToken:
                    throw new NotImplementedException();
                case UnaryOpToken:
                    throw new NotImplementedException();
                // * May be expected. Used in ternary expression and has lowest (after commas) precedence,
                //   so the tree should be returned with it as ender. 
                case ColonToken:
                // These will fail or end the expression, return it and |current| as sequence ender.
                // * any keyword except "var" is invalid token
                case KeywordToken:
                // * Unexpected, fail
                case FeatureToken:
                // * Unexpected, fail
                case FailedToken:
                // * May be expected. Can end valid expression
                case SemicolonToken:
                    return (tree, current);
                default:
                    throw new ArgumentOutOfRangeException(nameof(current));
            }
            i++;
        }

        return (tree, null);
    }
}