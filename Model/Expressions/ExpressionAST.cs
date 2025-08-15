using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Expressions;

internal abstract class BaseExpression(BaseExpression? parent) : IHasBounds, IVisualProvider
{
    public BaseExpression? Parent = parent;

    public abstract int Start { get; }
    public abstract int End { get; }

    protected virtual void Replace(BaseExpression target, FunctionCall value)
        => throw new Exception("Cannot replace child at the bottom-level node");

    public virtual void VerifySyntax(DiagnosticContext context) { }

    /// <summary>
    /// Try to parse an NML expression.
    /// </summary>
    /// <param name="state">The current parsing state.</param>
    /// <param name="parseUntilOuterComma">The flag to stop parsing if top-level comma is encountered.</param>
    /// <returns>The root of parsed expression, if any; null otherwise.</returns>
    /// <remarks>Any NML expression can contain at most one unit token. If if does, it is always the last token,
    /// and corresponding <see cref="UnitTerminatedExpression"/> node is the root of result.</remarks>
    public static BaseExpression? TryParse(ref ParsingState state, bool parseUntilOuterComma = false)
    {
        BaseExpression? result = null;
        BaseExpression? current = result;
        while (state.CurrentToken is { } token)
        {
            BaseExpression value;
            switch (token)
            {
                case CommentToken:
                    state.Increment();
                    continue;

                case UnitToken unitToken:
                    (result is null ? ref result : ref result.Parent) = new UnitTerminatedExpression(result, unitToken);
                    state.Increment();
                    return result;

                case ColonToken colonToken:
                    if (current is null)
                        return null;
                    {
                        if (current is TernaryOperation { Colon: null, FalseBranch: null } ternaryOp)
                        {
                            ternaryOp.Colon = colonToken;
                            break;
                        }
                    }
                    if (current.Parent is null)
                        return result;
                    current = current.Parent;
                    continue;

                case TernaryOpToken questionMark:
                    switch (current)
                    {
                        case null:
                            current = result = new TernaryOperation(null, questionMark);
                            break;

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
                        case BinaryOperation op when op.Precedence > TernaryOperation.Precedence:
                        case BaseValueNode:
                        case ParentedExpression:
                            if (current.Parent is null)
                            {
                                result = current = current.Parent = new TernaryOperation(null, current, questionMark);
                                break;
                            }
                            current = current.Parent;
                            continue;

                        case BinaryOperation comma:
                            current = comma.Right = new TernaryOperation(comma, comma.Right, questionMark);
                            break;
                    }
                    break;

                case BracketToken { Bracket: '(' or '[' } openingParen:
                    value = new ParentedExpression(current, openingParen);
                    switch (current)
                    {
                        case null:
                            current = result = new ParentedExpression(openingBracket: openingParen);
                            break;

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
                            var call = new FunctionCall(current.Parent, valueNode.Token);
                            var parens = new ParentedExpression(call, openingParen);
                            call.Arguments = parens;
                            if (current.Parent is null)
                                result = call;
                            else
                                current.Parent.Replace(current, call);
                            current = parens;
                            break;

                        case FunctionCall:
                        case ParentedExpression:
                            if (current.Parent is null) // For templates and realsprites usage
                                return result;
                            state.AddUnexpected(token);
                            break;
                    }
                    break;

                case BracketToken { Bracket: ')' or ']' } closingParen:
                    if (current is null)
                    {
                        current = result = new ParentedExpression(closingBracket: closingParen);
                        break;
                    }
                    if (current is ParentedExpression { ClosingBracket: null } openParen && openParen.Matches(closingParen))
                    {
                        openParen.ClosingBracket = closingParen;
                        if (current.Parent is FunctionCall)
                            current = current.Parent;
                        break;
                    }
                    if (current.Parent is null)
                    {
                        current = result = result!.Parent = new ParentedExpression(null, null, result, closingParen);
                        break;
                    }
                    current = current.Parent;
                    continue;

                case RangeToken:
                case UnknownToken:
                case BracketToken:
                case KeywordToken { IsExpressionUsable: false }:
                case SemicolonToken:
                case AssignmentToken:
                    return result;

                case KeywordToken keywordToken:
                    value = new FunctionCall(current, keywordToken);
                    goto label_OnValue;
                case UnaryOpToken unaryOpToken:
                    value = new UnaryOperation(current, unaryOpToken);
                    goto label_OnValue;
                case IdentifierToken tok:
                    value = new Identifier(current, tok);
                    goto label_OnValue;
                case NumericToken tok:
                    value = new Number(current, tok);
                    goto label_OnValue;
                case StringToken tok:
                    value = new LiteralString(current, tok);
                    goto label_OnValue;
                label_OnValue:
                    switch (current)
                    {
                        case null:
                            current = result = value;
                            break;

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
                            state.AddUnexpected(token);
                            break;

                        case BinaryOperation:
                        case ParentedExpression:
                        case FunctionCall:
                        case UnaryOperation:
                            if (current.Parent is null)
                                return result;
                            state.AddUnexpected(token);
                            break;
                    }
                    break;

                case BinaryOpToken binaryOpToken:
                    switch (current)
                    {
                        case null:
                            if (parseUntilOuterComma & binaryOpToken.Type is OperatorType.Comma)
                                return null;
                            current = result = new BinaryOperation(null, binaryOpToken);
                            break;

                        case BinaryOperation binaryOp when binaryOpToken.Type.Precedence() > binaryOp.Precedence:
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
                        case TernaryOperation when binaryOpToken.Type.Precedence() < TernaryOperation.Precedence:
                        case ParentedExpression:
                            if (current.Parent is not null)
                            {
                                current = current.Parent;
                                continue;
                            }
                            if (parseUntilOuterComma & binaryOpToken.Type is OperatorType.Comma)
                                return result;
                            current = result = current.Parent = new BinaryOperation(null, current, binaryOpToken);
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
        return result;
    }

    public abstract DotNode Visualize(DotGraph graph, DotNode parent, string ctx);
}