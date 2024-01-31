using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal abstract class ExpressionAST
{
    private ExpressionAST? _parent;

    protected ExpressionAST(ExpressionAST? parent) => _parent = parent;

    protected virtual void Replace(ExpressionAST target, ExpressionAST value)
    {
        throw new Exception($"Cannot replace child: {GetType()} is a bottom-level node");
    }

    /// <summary>
    /// <para>Try to parse an NML expression.</para>
    /// NML expression can only contain ONE unit token. If if does, it is always the last token,
    /// and corresponding <see cref="UnitTerminatedExpression"/> node is the root of result.
    /// </summary>
    /// <param name="state">The current parsing state.</param>
    /// <param name="parseUntilOuterComma">The flag to stop parsing if top-level comma is encountered.</param>
    /// <returns>The root of parsed expression, if any; null otherwise.</returns>
    public static ExpressionAST? TryParse(ParsingState state, bool parseUntilOuterComma = false)
    {
        var token = state.currentToken;
        if (token is null || parseUntilOuterComma && token is BinaryOpToken { Type: OperatorType.Comma })
        {
            return null;
        }

        var result = TokenToAST(token);
        switch (result)
        {
            case null:
                return null;

            case UnitTerminatedExpression:
                state.Increment();
                return result;
        }
        ExpressionAST current = result;

        for (token = state.nextToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case CommentToken:
                    state.Increment();
                    continue;

                case UnitToken unitToken:
                    result._parent = new UnitTerminatedExpression(result, unitToken);
                    state.Increment();
                    return result;

                case ColonToken colonToken:
                    switch (current)
                    {
                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOperation:
                            ternaryOperation.Colon = colonToken;
                            break;
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case TernaryOperation:
                        case BaseValueNode:
                        case ParentedExpression:
                            if (current._parent is null)
                            {
                                return result;
                            }
                            current = current._parent;
                            continue;
                    }
                    break;

                case TernaryOpToken questionMark:
                    switch (current)
                    {
                        case ParentedExpression { ClosingBracket: null } openBrackets:
                            openBrackets.Expression = new TernaryOperation(openBrackets, openBrackets.Expression,
                                questionMark);
                            current = openBrackets.Expression;
                            break;

                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOperation:
                            ternaryOperation.TrueBranch = new TernaryOperation(ternaryOperation,
                                ternaryOperation.TrueBranch, questionMark);
                            current = ternaryOperation.TrueBranch;
                            break;

                        case TernaryOperation ternaryOperation:
                            ternaryOperation.FalseBranch = new TernaryOperation(ternaryOperation,
                                ternaryOperation.FalseBranch, questionMark);
                            current = ternaryOperation.FalseBranch;
                            break;

                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation { Precedence: > TernaryOpToken.Precedence }:
                        case BaseValueNode:
                        case ParentedExpression:
                            if (current._parent is null)
                            {
                                current._parent = new TernaryOperation(null, current, questionMark);
                                current = current._parent;
                                result = current;
                                break;
                            }
                            current = current._parent;
                            continue;

                        case BinaryOperation comma:
                            comma.Right = new TernaryOperation(comma, comma.Right, questionMark);
                            current = comma.Right;
                            break;
                    }
                    break;

                case BracketToken { Bracket: '(' or '[' } openingParen:
                {
                    switch (current)
                    {
                        // opening BracketToken pulls down, on TernaryExpression level => TrueBranch is null
                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOperation:
                            ternaryOperation.TrueBranch = new ParentedExpression(ternaryOperation, openingParen);
                            current = ternaryOperation.TrueBranch;
                            break;

                        case TernaryOperation ternaryOperation:
                            ternaryOperation.FalseBranch = new ParentedExpression(ternaryOperation, openingParen);
                            current = ternaryOperation.FalseBranch;
                            break;

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

                        case BaseValueNode valueNode:
                            var call = new FunctionCall(current._parent, valueNode.Token);
                            var parens = new ParentedExpression(call, openingParen);
                            call.Arguments = parens;
                            if (current._parent is null)
                            {
                                result = call;
                            }
                            else
                            {
                                current._parent.Replace(current, call);
                            }
                            current = parens;
                            break;

                        case FunctionCall:
                        case ParentedExpression:
                            return result;
                    }
                    break;
                }

                case BracketToken { Bracket: ')' or ']' } closingParen:
                    switch (current)
                    {
                        case ParentedExpression { ClosingBracket: null } openParen when openParen.Matches(closingParen):
                            openParen.ClosingBracket = closingParen;
                            if (current._parent is FunctionCall)
                            {
                                current = current._parent;
                            }
                            break;

                        case BaseValueNode:
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case TernaryOperation:
                        case ParentedExpression:
                            if (current._parent is null)
                            {
                                var next = new ParentedExpression(null, null)
                                {
                                    Expression = result,
                                    ClosingBracket = closingParen,
                                };
                                result._parent = next;
                                result = result._parent;
                                current = result;
                                break;
                            }
                            current = current._parent;
                            continue;
                    }
                    break;

                case RangeToken:
                case FailedToken:
                case BracketToken:
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                case SemicolonToken:
                case AssignmentToken:
                    return result;

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

                        // KeywordToken pulls down, on TernaryExpression level => TrueBranch is null
                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOperation:
                            ternaryOperation.TrueBranch = new FunctionCall(ternaryOperation, keywordToken);
                            current = ternaryOperation.TrueBranch;
                            break;

                        case TernaryOperation ternaryOperation:
                            ternaryOperation.FalseBranch = new FunctionCall(ternaryOperation, keywordToken);
                            current = ternaryOperation.FalseBranch;
                            break;

                        case BaseValueNode:
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case ParentedExpression:
                            return result;
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

                        case ParentedExpression { Expression: null, ClosingBracket: null } parentedExpression:
                            parentedExpression.Expression = new UnaryOperation(parentedExpression, unaryOpToken);
                            current = parentedExpression.Expression;
                            break;

                        // UnaryOpToken pulls down, on TernaryExpression level => TrueBranch is null
                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOperation:
                            ternaryOperation.TrueBranch = new UnaryOperation(ternaryOperation, unaryOpToken);
                            current = ternaryOperation.TrueBranch;
                            break;

                        case TernaryOperation ternaryOperation:
                            ternaryOperation.FalseBranch = new UnaryOperation(ternaryOperation, unaryOpToken);
                            current = ternaryOperation.FalseBranch;
                            break;

                        case BaseValueNode:
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case ParentedExpression:
                            return result;
                    }
                    break;

                case BaseValueToken valueToken:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOperation:
                            unaryOperation.Expression = ValueNodeFactory.Make(unaryOperation, valueToken);
                            current = unaryOperation.Expression;
                            break;

                        case BinaryOperation { Right: null } binaryOperation:
                            binaryOperation.Right = ValueNodeFactory.Make(binaryOperation, valueToken);
                            current = binaryOperation.Right;
                            break;

                        // BaseValueToken pulls down, on TernaryExpression level => TrueBranch/FalseBranch is null
                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOperation:
                            ternaryOperation.TrueBranch = ValueNodeFactory.Make(ternaryOperation, valueToken);
                            current = ternaryOperation.TrueBranch;
                            break;

                        case TernaryOperation ternaryOperation:
                            ternaryOperation.FalseBranch = ValueNodeFactory.Make(ternaryOperation, valueToken);
                            current = ternaryOperation.FalseBranch;
                            break;

                        case ParentedExpression { Expression: null, ClosingBracket: null } openParen:
                            openParen.Expression = ValueNodeFactory.Make(openParen, valueToken);
                            current = openParen.Expression;
                            break;

                        case BaseValueNode:
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case ParentedExpression:
                            return result;
                    }
                    break;

                case BinaryOpToken binaryOpToken:
                    switch (current)
                    {
                        case BinaryOperation binaryOperation when binaryOpToken.Precedence > binaryOperation.Precedence:
                            binaryOperation.Right = new BinaryOperation(binaryOperation, binaryOperation.Right,
                                binaryOpToken);
                            current = binaryOperation.Right;
                            break;

                        case ParentedExpression { ClosingBracket: null } parentedExpression:
                            parentedExpression.Expression = new BinaryOperation(parentedExpression,
                                parentedExpression.Expression, binaryOpToken);
                            current = parentedExpression.Expression;
                            break;

                        case BaseValueNode:
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case TernaryOperation when binaryOpToken.Precedence < TernaryOperation.Precedence:
                        case ParentedExpression:
                            if (current._parent is null)
                            {
                                if (parseUntilOuterComma && binaryOpToken.Type is OperatorType.Comma)
                                {
                                    return result;
                                }
                                current._parent = new BinaryOperation(null, current, binaryOpToken);
                                current = current._parent;
                                result = current;
                                break;
                            }
                            current = current._parent;
                            continue;

                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOperation:
                            ternaryOperation.TrueBranch = new BinaryOperation(ternaryOperation,
                                ternaryOperation.TrueBranch, binaryOpToken);
                            current = ternaryOperation.TrueBranch;
                            break;

                        case TernaryOperation ternaryOperation:
                            ternaryOperation.FalseBranch = new BinaryOperation(ternaryOperation,
                                ternaryOperation.FalseBranch, binaryOpToken);
                            current = ternaryOperation.FalseBranch;
                            break;
                    }
                    break;
            }
            state.Increment();
        }
        return null;
    }

    private static ExpressionAST? TokenToAST(Token token)
    {
        return token switch
        {
            KeywordToken keywordToken => keywordToken.Kind is KeywordKind.ExpressionUsable
                ? new FunctionCall(null, keywordToken)
                : null,
            StringToken stringToken => new LiteralString(null, stringToken),
            NumericToken numericToken => new Number(null, numericToken),
            IdentifierToken literalToken => new Identifier(null, literalToken),
            BinaryOpToken binaryOpToken => new BinaryOperation(null, binaryOpToken),
            UnaryOpToken unaryOpToken => new UnaryOperation(null, unaryOpToken),
            TernaryOpToken ternaryOpToken => new TernaryOperation(null, ternaryOpToken),
            BracketToken bracketToken => bracketToken.Bracket switch
            {
                '(' => new ParentedExpression(null, bracketToken),
                '[' => new ParentedExpression(null, bracketToken),
                ')' => new ParentedExpression(bracketToken),
                ']' => new ParentedExpression(bracketToken),
                _ => null
            },
            UnitToken unitToken => new UnitTerminatedExpression(null, unitToken),
            _ => null
        };
    }
}