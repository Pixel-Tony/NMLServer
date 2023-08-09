using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class ExpressionParser
{
    private static ExpressionAST? FromToken(Token token)
    {
        return token switch
        {
            LiteralToken literalToken => new Variable(null, literalToken),
            NumericToken numericToken => new Number(null, numericToken),
            BinaryOpToken binaryOpToken => new BinaryOperation(null, binaryOpToken),
            BaseRecordingToken baseRecordingToken => throw new NotImplementedException(),
            BracketToken bracketToken => throw new NotImplementedException(),
            ColonToken colonToken => throw new NotImplementedException(),
            FailedToken failedToken => throw new NotImplementedException(),
            FeatureToken featureToken => throw new NotImplementedException(),
            KeywordToken keywordToken => throw new NotImplementedException(),
            SemicolonToken semicolonToken => throw new NotImplementedException(),
            TernaryOpToken ternaryOpToken => throw new NotImplementedException(),
            UnaryOpToken unaryOpToken => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };
    }

    public (ExpressionAST?, Token?) Parse(Token[] tokens, int start)
    {
        int i = start;
        int max = tokens.Length;

        if (i >= max)
        {
            return (null, null);
        }
        var root = FromToken(tokens[i]);
        i++;
        if (root == null)
        {
            return (null, tokens[i]);
        }
        ExpressionAST current = root;

        while (i < max)
        {
            var token = tokens[i];
            Console.WriteLine($"Parsing on {token};");
            switch (token)
            {
                case LiteralToken literalToken:
                    switch (current)
                    {
                        // Two literals in a raw is a fail, return
                        case Variable:
                            return (root, token);

                        case BinaryOperation binaryOperation:
                            if (binaryOperation.Right != null)
                            {
                                return (root, token);
                            }
                            binaryOperation.Right = new Variable(binaryOperation, literalToken);
                            break;
                    }
                    break;
                case BinaryOpToken binaryOpToken:
                    switch (current)
                    {
                        case BinaryOperation binaryOperation:
                            if (binaryOperation.Right == null)
                            {
                                throw new NotImplementedException("Failing scheme");
                            }
                            else if (binaryOpToken.precedence > binaryOperation.Operation.precedence)
                            {
                                var right = new BinaryOperation(binaryOperation, binaryOpToken)
                                {
                                    Left = binaryOperation.Right
                                };
                                binaryOperation.Right = right;
                                current = right;
                            }
                            else
                            {
                                // TODO: propagate until precedence is less, then do tree rotation
                                throw new NotImplementedException();
                            }
                            break;
                        case Variable:
                            if (current.Parent == null)
                            {
                                var next = new BinaryOperation(null, binaryOpToken)
                                {
                                    Left = current
                                };
                                current.Parent = next;
                                current = next;
                                root = current;
                            }
                            else
                            {
                                current = current.Parent.Replace(current,
                                    new BinaryOperation(current, binaryOpToken));
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }

            i++;
        }

        return (root, null);
    }
}