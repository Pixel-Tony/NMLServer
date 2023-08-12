using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;
using static NMLServer.Parsing.Grammar;

namespace NMLServer.Parsing.Expression;

internal class ExpressionParser
{
    private readonly Token[] _tokens;
    private readonly int _max;

    public ExpressionParser(Token[] tokens)
    {
        _tokens = tokens;
        _max = _tokens.Length;
    }

    // BinaryOpToken BracketToken ColonToken CommentToken FailedToken FeatureToken KeywordToken LiteralToken
    // NumericToken SemicolonToken StringToken TernaryOpToken UnaryOpToken

    // BracketToken ColonToken FailedToken FeatureToken KeywordToken SemicolonToken TernaryOpToken

    public (ExpressionAST?, Token?) Parse(int start)
    {
        int i = start;
        while (i < _max && _tokens[i] is CommentToken)
        {
            i++;
        }
        if (i >= _max)
        {
            return (null, null);
        }
        var firstToken = _tokens[i];
        var root = TokenToAST(firstToken);
        if (root == null)
        {
            return (null, firstToken);
        }
        ExpressionAST current = root;
        i++;

        while (i < _max)
        {
            var token = _tokens[i];
            // Console.WriteLine($"Parsing on {token};");
            switch (token)
            {
                case ColonToken colonToken:
                    switch (current)
                    {
                        case TernaryOperation { Colon: null } ternaryOperation:
                            ternaryOperation.Colon = colonToken;
                            break;
                        case TernaryOperation:
                        case FunctionCall:
                        case ParentedExpression:
                        case BinaryOperation:
                        case UnaryOperation:
                        case IHoldsSingleToken:
                            if (current.Parent == null)
                            {
                                return (root, token);
                            }
                            current = current.Parent;
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case FailedToken:
                    return (root, token);
                case TernaryOpToken:
                    throw new NotImplementedException();
                case BracketToken { Bracket: '(' or '[' } openingParen:
                {
                    switch (current)
                    {
                        case TernaryOperation:
                            throw new NotImplementedException();
                        case FunctionCall { Arguments: null } futureCall:
                            futureCall.Arguments = new ParentedExpression(futureCall, openingParen);
                            current = futureCall.Arguments;
                            break;
                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = new ParentedExpression(binaryOperation, openingParen);
                            current = binaryOperation.Right;
                            break;
                        case BinaryOperation binaryOperation:
                            current = binaryOperation.Right;
                            continue;
                        case ParentedExpression { ClosingBracket: null, Expression: null } openedParen:
                            openedParen.Expression = new ParentedExpression(openedParen, openingParen);
                            current = openedParen.Expression;
                            break;
                        case ParentedExpression { ClosingBracket: null } parentedExpression:
                            current = parentedExpression.Expression;
                            continue;
                        case ParentedExpression:
                        case FunctionCall:
                            return (root, token);
                        case UnaryOperation { Expression: null } unaryOperation:
                        {
                            unaryOperation.Expression = new ParentedExpression(unaryOperation, openingParen);
                            current = unaryOperation.Expression;
                            break;
                        }
                        case UnaryOperation unaryOperation:
                        {
                            current = unaryOperation.Expression;
                            continue;
                        }
                        case IHoldsSingleToken valueNode:
                            var call = new FunctionCall(current.Parent, valueNode.token);
                            var parens = new ParentedExpression(call, openingParen);
                            call.Arguments = parens;
                            if (current.Parent == null)
                            {
                                root = call;
                            }
                            else
                            {
                                current.Parent.Replace(current, call);
                            }
                            current = parens;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                }
                case BracketToken { Bracket: ')' or ']' } closingParen:
                    switch (current)
                    {
                        case ParentedExpression { Expression: null, ClosingBracket: null } openParen:
                            openParen.ClosingBracket = closingParen;
                            break;
                        case ParentedExpression { ClosingBracket: null } parentedExpression:
                            parentedExpression.ClosingBracket = closingParen;
                            break;
                        case UnaryOperation:
                        case BinaryOperation:
                        case ParentedExpression:
                        case FunctionCall:
                        case IHoldsSingleToken:
                            if (current.Parent == null)
                            {
                                return (root, token);
                            }
                            current = current.Parent;
                            continue;
                        case TernaryOperation:
                            throw new NotImplementedException();
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case BracketToken { Bracket: '{' or '}' }:
                case SemicolonToken:
                case KeywordToken keywordToken when NotExpressionKeywords.Contains(keywordToken.value):
                    return (root, token);
                case KeywordToken keywordToken:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOperation:
                            unaryOperation.Expression = new FunctionCall(unaryOperation, keywordToken);
                            current = unaryOperation.Expression;
                            break;
                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = new FunctionCall(binaryOperation, keywordToken);
                            current = binaryOperation.Right;
                            break;
                        case ParentedExpression { Expression: null, ClosingBracket: null } parentedExpression:
                            parentedExpression.Expression = new FunctionCall(parentedExpression, keywordToken);
                            current = parentedExpression.Expression;
                            break;
                        case ParentedExpression:
                        case UnaryOperation:
                        case BinaryOperation:
                        case IHoldsSingleToken:
                        case FunctionCall:
                            return (root, token);
                        case TernaryOperation:
                            throw new NotImplementedException();
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case UnaryOpToken unaryOpToken:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOperation:
                            unaryOperation.Expression = new UnaryOperation(unaryOperation, unaryOpToken);
                            current = unaryOperation.Expression;
                            break;
                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = new UnaryOperation(binaryOperation, unaryOpToken);
                            current = binaryOperation.Right;
                            break;
                        case FunctionCall { Arguments: null }:
                            return (root, token);
                        case FunctionCall:
                            throw new NotImplementedException();
                        case ParentedExpression:
                            throw new NotImplementedException();
                        case TernaryOperation:
                            throw new NotImplementedException();
                        case UnaryOperation:
                        case BinaryOperation:
                        case IHoldsSingleToken:
                            return (root, token);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case BaseValueToken valueToken:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOperation:
                            unaryOperation.Expression = valueToken.ToAST(unaryOperation);
                            current = unaryOperation.Expression;
                            break;
                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = valueToken.ToAST(binaryOperation);
                            current = binaryOperation.Right;
                            break;
                        case TernaryOperation:
                            throw new NotImplementedException();
                        case ParentedExpression { Expression: null, ClosingBracket: null } openParen:
                            openParen.Expression = valueToken.ToAST(openParen);
                            current = openParen.Expression;
                            break;
                        case ParentedExpression:
                            return (root, token);
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case IHoldsSingleToken:
                            return (root, token);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case BinaryOpToken binaryOpToken:
                    switch (current)
                    {
                        case BinaryOperation binaryOperation when binaryOpToken > binaryOperation:
                        {
                            binaryOperation.Right = new BinaryOperation(binaryOperation, binaryOperation.Right,
                                binaryOpToken);
                            current = binaryOperation.Right;
                            break;
                        }
                        case BinaryOperation:
                        case IHoldsSingleToken:
                        case UnaryOperation:
                        case FunctionCall:
                            if (current.Parent == null)
                            {
                                current.Parent = new BinaryOperation(null, current, binaryOpToken);
                                current = current.Parent;
                                root = current;
                                break;
                            }
                            current = current.Parent;
                            continue;
                        case ParentedExpression { ClosingBracket: null } parentedExpression:
                            parentedExpression.Expression = new BinaryOperation(parentedExpression,
                                parentedExpression.Expression, binaryOpToken);
                            current = parentedExpression.Expression;
                            break;
                        case ParentedExpression:
                            if (current.Parent == null)
                            {
                                current.Parent = new BinaryOperation(null, current, binaryOpToken);
                                current = current.Parent;
                                root = current;
                                break;
                            }
                            current = current.Parent;
                            continue;
                        case TernaryOperation:
                            throw new NotImplementedException();
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case CommentToken:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
            i++;
        }
        return (root, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExpressionAST? KeywordTokenToAST(BaseRecordingToken token)
    {
        string value = token.value ?? throw new Exception();
        return ExpressionKeywords.ContainsKey(value)
            ? new FunctionCall(null, token)
            : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExpressionAST? TokenToAST(Token token)
    {
        return token switch
        {
            KeywordToken keywordToken => KeywordTokenToAST(keywordToken),
            StringToken stringToken => new LiteralString(null, stringToken),
            NumericToken numericToken => new Number(null, numericToken),
            LiteralToken literalToken => new Identifier(null, literalToken),
            BinaryOpToken binaryOpToken => new BinaryOperation(null, binaryOpToken),
            UnaryOpToken unaryOpToken => new UnaryOperation(null, unaryOpToken),
            TernaryOpToken ternaryOpToken => new TernaryOperation(null, ternaryOpToken),
            BracketToken bracketToken => bracketToken.Bracket switch
            {
                '(' => new ParentedExpression(null, bracketToken),
                '[' => new ParentedExpression(null, bracketToken),
                ')' => null,
                '{' => null,
                '}' => null,
                ']' => null,
                _ => throw new ArgumentOutOfRangeException()
            },
            ColonToken => null,
            FailedToken => null,
            SemicolonToken => null,
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };
    }
}