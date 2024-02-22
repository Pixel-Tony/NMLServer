using NMLServer.Lexing;
using NMLServer.Model.Statement;

namespace NMLServer.Model.Expression;

internal abstract class ExpressionAST : IAllowsParseInsideBlock<ExpressionAST>
{
    private ExpressionAST? _parent;

    public abstract int start { get; }
    public abstract int end { get; }

    protected ExpressionAST(ExpressionAST? parent) => _parent = parent;

    protected virtual void Replace(ExpressionAST target, FunctionCall value)
    {
        throw new Exception($"Cannot replace child: {GetType()} is a bottom-level node");
    }

    public static List<ExpressionAST>? ParseSomeInBlock(ParsingState state, ref BracketToken? closingBracket)
    {
        var expression = TryParse(state);
        if (expression is not null)
        {
            List<ExpressionAST> result = new();
            while (expression is not null)
            {
                result.Add(expression);
                expression = TryParse(state);
            }
            closingBracket = state.ExpectClosingCurlyBracket();
            return result;
        }
        closingBracket = state.ExpectClosingCurlyBracket();
        return null;
    }

    // TODO: possibly provide Parse(ParsingState, bool, Token) method for parsing from already checked token
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
            ExpressionAST value;
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
                {
                    if (current is TernaryOperation { Colon: null, FalseBranch: null } ternaryOp)
                    {
                        ternaryOp.Colon = colonToken;
                        break;
                    }
                    if (current._parent is null)
                    {
                        return result;
                    }
                    current = current._parent;
                    continue;
                }

                case TernaryOpToken questionMark:
                    switch (current)
                    {
                        case ParentedExpression { ClosingBracket: null } parented:
                            current = parented.Expression
                                = new TernaryOperation(current, parented.Expression, questionMark);
                            break;

                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOp:
                            current = ternaryOp.TrueBranch
                                = new TernaryOperation(current, ternaryOp.TrueBranch, questionMark);
                            break;

                        case TernaryOperation ternaryOperation:
                            current = ternaryOperation.FalseBranch
                                = new TernaryOperation(current, ternaryOperation.FalseBranch, questionMark);
                            break;

                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation { precedence: > Grammar.TernaryOperatorPrecedence }:
                        case BaseValueNode:
                        case ParentedExpression:
                            if (current._parent is null)
                            {
                                result = current = current._parent = new TernaryOperation(null, current, questionMark);
                                break;
                            }
                            current = current._parent;
                            continue;

                        case BinaryOperation comma:
                            current = comma.Right = new TernaryOperation(comma, comma.Right, questionMark);
                            break;
                    }
                    break;

                case BracketToken { Bracket: '(' or '[' } openingParen:
                {
                    value = new ParentedExpression(current, openingParen);
                    switch (current)
                    {
                        // opening BracketToken pulls down, on TernaryExpression level => TrueBranch is null
                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOp:
                            current = ternaryOp.TrueBranch = value;
                            break;

                        case TernaryOperation ternaryOp:
                            current = ternaryOp.FalseBranch = value;
                            break;

                        case FunctionCall { Arguments: null } futureCall:
                            current = futureCall.Arguments = new ParentedExpression(current, openingParen);
                            break;

                        case BinaryOperation { Right: null } binaryOperation:
                            current = binaryOperation.Right = value;
                            break;

                        case BinaryOperation binaryOperation:
                            current = binaryOperation.Right;
                            continue;

                        case ParentedExpression { ClosingBracket: null, Expression: null } parented:
                            current = parented.Expression = value;
                            break;

                        case ParentedExpression { ClosingBracket: null } parented:
                            current = parented.Expression;
                            continue;

                        case UnaryOperation { Expression: null } unaryOperation:
                            current = unaryOperation.Expression = value;
                            break;

                        case UnaryOperation unaryOperation:
                            current = unaryOperation.Expression;
                            continue;

                        case BaseValueNode valueNode:
                            var call = new FunctionCall(current._parent, valueNode.token);
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
                    if (current is ParentedExpression { ClosingBracket: null } openParen
                        && openParen.Matches(closingParen))
                    {
                        openParen.ClosingBracket = closingParen;
                        if (current._parent is FunctionCall)
                        {
                            current = current._parent;
                        }
                        break;
                    }
                    if (current._parent is null)
                    {
                        current = result = result._parent = new ParentedExpression(null, null, result, closingParen);
                        break;
                    }
                    current = current._parent;
                    continue;

                case RangeToken:
                case UnknownToken:
                case BracketToken:
                case KeywordToken { Kind: not KeywordKind.ExpressionUsable }:
                case SemicolonToken:
                case AssignmentToken:
                    return result;

                case KeywordToken keywordToken:
                    value = new FunctionCall(current, keywordToken);
                    goto label_OnValue;
                case UnaryOpToken unaryOpToken:
                    value = new UnaryOperation(current, unaryOpToken);
                    goto label_OnValue;
                case BaseValueToken valueToken:
                    value = MakeValueFromToken(current, valueToken);

                    label_OnValue:
                    switch (current)
                    {
                        case UnaryOperation { Expression: null } unaryOp:
                            current = unaryOp.Expression = value;
                            break;

                        case BinaryOperation { Right: null } binaryOp:
                            current = binaryOp.Right = value;
                            break;

                        // token pulls down, on TernaryExpression level => TrueBranch is null
                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOp:
                            current = ternaryOp.TrueBranch = value;
                            break;

                        case TernaryOperation ternaryOp:
                            current = ternaryOp.FalseBranch = value;
                            break;

                        case ParentedExpression { Expression: null, ClosingBracket: null } parented:
                            current = parented.Expression = value;
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
                        case BinaryOperation binaryOp when binaryOpToken.Precedence > binaryOp.precedence:
                            current = binaryOp.Right = new BinaryOperation(current, binaryOp.Right, binaryOpToken);
                            break;

                        case ParentedExpression { ClosingBracket: null } parented:
                            current = parented.Expression
                                = new BinaryOperation(current, parented.Expression, binaryOpToken);
                            break;

                        case BaseValueNode:
                        case FunctionCall:
                        case UnaryOperation:
                        case BinaryOperation:
                        case TernaryOperation when binaryOpToken.Precedence < TernaryOperation.Precedence:
                        case ParentedExpression:
                            if (current._parent is not null)
                            {
                                current = current._parent;
                                continue;
                            }
                            if (parseUntilOuterComma && binaryOpToken.Type is OperatorType.Comma)
                            {
                                return result;
                            }
                            current = result = current._parent = new BinaryOperation(null, current, binaryOpToken);
                            break;

                        case TernaryOperation { Colon: null, FalseBranch: null } ternaryOp:
                            current = ternaryOp.TrueBranch
                                = new BinaryOperation(current, ternaryOp.TrueBranch, binaryOpToken);
                            break;

                        case TernaryOperation ternaryOp:
                            current = ternaryOp.FalseBranch
                                = new BinaryOperation(current, ternaryOp.FalseBranch, binaryOpToken);
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
            UnitToken unitToken => new UnitTerminatedExpression(null, unitToken),
            UnaryOpToken unaryOpToken => new UnaryOperation(null, unaryOpToken),
            BinaryOpToken binaryOpToken => new BinaryOperation(null, binaryOpToken),
            TernaryOpToken ternaryOpToken => new TernaryOperation(null, ternaryOpToken),
            BaseValueToken valueToken => MakeValueFromToken(null, valueToken),
            KeywordToken keywordToken => keywordToken.Kind is KeywordKind.ExpressionUsable
                ? new FunctionCall(null, keywordToken)
                : null,
            BracketToken bracketToken => bracketToken.Bracket switch
            {
                '(' or '[' => new ParentedExpression(openingBracket: bracketToken),
                ')' or ']' => new ParentedExpression(closingBracket: bracketToken),
                _ => null
            },
            _ => null
        };
    }

    private static BaseValueNode MakeValueFromToken(ExpressionAST? parent, BaseValueToken token)
    {
        return token switch
        {
            IdentifierToken tok => new Identifier(parent, tok),
            NumericToken tok => new Number(parent, tok),
            StringToken tok => new LiteralString(parent, tok),
            _ => throw new ArgumentOutOfRangeException(nameof(token), "Unexpected token type")
        };
    }

    public virtual void ProvideDiagnostics(DiagnosticsContext context)
    {
        // TODO
    }
}