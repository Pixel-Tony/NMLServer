#define DEBUG_CONSOLE
using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;

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

    // Array FunctionCall Keyword Parens Ternary

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
        var root = TokenToAST(_tokens[i]);
        i++;
        if (root == null)
        {
            return (null, _tokens[i]);
        }
        ExpressionAST current = root;

        while (i < _max)
        {
            var token = _tokens[i];
#if DEBUG_CONSOLE
            Console.WriteLine($"Parsing on {token};");
#endif
            switch (token)
            {
                case KeywordToken keywordToken:
                    if (Grammar.NotExpressionKeywords.Contains(keywordToken.value))
                    {
                        return (root, token);
                    }
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
                        case UnaryOperation:
                        case BinaryOperation:
                        case LiteralString:
                        case Number:
                        case Identifier:
                            return (root, token);
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
                        case UnaryOperation:
                        case BinaryOperation:
                        case LiteralString:
                        case Number:
                        case Identifier:
                            return (root, token);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case LiteralToken literalToken:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOperation:
                            unaryOperation.Expression = new Identifier(unaryOperation, literalToken);
                            break;
                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = new Identifier(binaryOperation, literalToken);
                            break;
                        case UnaryOperation:
                        case BinaryOperation:
                        case LiteralString:
                        case Number:
                        case Identifier:
                            return (root, token);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case NumericToken numericToken:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOperation:
                            unaryOperation.Expression = new Number(unaryOperation, numericToken);
                            break;
                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = new Number(binaryOperation, numericToken);
                            break;
                        case UnaryOperation:
                        case BinaryOperation:
                        case LiteralString:
                        case Number:
                        case Identifier:
                            return (root, token);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case StringToken stringToken:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOperation:
                            unaryOperation.Expression = new LiteralString(unaryOperation, stringToken);
                            break;
                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = new LiteralString(binaryOperation, stringToken);
                            break;
                        case UnaryOperation:
                        case BinaryOperation:
                        case LiteralString:
                        case Number:
                        case Identifier:
                            return (root, token);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case BinaryOpToken binaryOpToken:
                    switch (current)
                    {
                        case UnaryOperation unaryOperation:
                            if (current.Parent == null)
                            {
                                var next = new BinaryOperation(null, unaryOperation, binaryOpToken);
                                current.Parent = next;
                                current = next;
                                root = current;
                                break;
                            }
                            current = current.Parent;
                            continue;
                        case BinaryOperation binaryOperation:
                            if (binaryOpToken.precedence > binaryOperation.Operation.precedence)
                            {
                                var right = new BinaryOperation(binaryOperation, binaryOperation.Right, binaryOpToken);
                                binaryOperation.Right = right;
                                current = right;
                                break;
                            }
                            if (current.Parent == null)
                            {
                                var next = new BinaryOperation(null, binaryOperation, binaryOpToken);
                                current.Parent = next;
                                current = next;
                                root = current;
                                break;
                            }
                            current = current.Parent;
                            continue;
                        case LiteralString:
                        case Number:
                        case Identifier:
                            if (current.Parent == null)
                            {
                                var next = new BinaryOperation(null, current, binaryOpToken);
                                current.Parent = next;
                                current = next;
                                root = current;
                                break;
                            }
                            current = current.Parent.Replace(current, new BinaryOperation(current, binaryOpToken));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                case CommentToken:
                    break;
                case SemicolonToken:
                    return (root, token);
                case BracketToken:
                case ColonToken:
                case FailedToken:
                case FeatureToken:
                case TernaryOpToken:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
            i++;
        }
        return (root, null);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExpressionAST? LiteralTokenToAST(BaseRecordingToken token)
    {
        if (token.value == null)
        {
            throw new Exception();
        }
        string value = token.value;

        // Features and most keywords cannot be used in expressions
        if (Grammar.Features.Contains(value) || Grammar.NotExpressionKeywords.Contains(value))
        {
            return null;
        }
        return Grammar.ExpressionKeywords.ContainsKey(value)
            ? new FunctionCall(null, token)
            : new Identifier(null, token);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExpressionAST? TokenToAST(Token token)
    {
        return token switch
        {
            BinaryOpToken binaryOpToken => new BinaryOperation(null, binaryOpToken),
            UnaryOpToken unaryOpToken => new UnaryOperation(null, unaryOpToken),
            LiteralToken literalToken => LiteralTokenToAST(literalToken),
            KeywordToken keywordToken => LiteralTokenToAST(keywordToken),
            NumericToken numericToken => new Number(null, numericToken),
            StringToken stringToken => new LiteralString(null, stringToken),
            TernaryOpToken ternaryOpToken => new TernaryOperation(null, ternaryOpToken),
            BracketToken bracketToken => bracketToken.Bracket switch
            {
                '(' => new ParentedExpression(null, bracketToken),
                '[' => new ArrayExpression(null, bracketToken),
                ')' => null,
                '{' => null,
                '}' => null,
                ']' => null,
                _ => throw new ArgumentOutOfRangeException()
            },
            ColonToken => null,
            FailedToken => null,
            FeatureToken => null,
            SemicolonToken => null,
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };
    }
}