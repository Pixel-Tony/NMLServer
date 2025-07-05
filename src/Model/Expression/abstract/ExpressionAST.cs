using DotNetGraph.Core;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
using NMLServer.Model.Statement;

namespace NMLServer.Model.Expression;

internal abstract class ExpressionAST(ExpressionAST? parent)
    : IHasStart, IAllowsParseInsideBlock<ExpressionAST>, IDiagnosticProvider
{
    public static class Errors
    {
        public const string ErrorMissingExpr = "Missing expression";
    }

    private ExpressionAST? _parent = parent;

    public abstract int Start { get; }
    public abstract int End { get; }

    protected virtual void Replace(ExpressionAST target, FunctionCall value)
        => throw new Exception("Cannot replace child at the bottom-level node");

    public abstract DotNode Visualize(DotGraph graph, DotNode parent, string ctx);

    public static List<ExpressionAST>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
    {
        var expression = TryParse(ref state);
        if (expression is not null)
        {
            List<ExpressionAST> result = [];
            while (expression is not null)
            {
                result.Add(expression);
                expression = TryParse(ref state);
            }
            closingBracket = state.ExpectClosingCurlyBracket();
            return result;
        }
        closingBracket = state.ExpectClosingCurlyBracket();
        return null;
    }

    /// <summary>
    /// Try to parse an NML expression.
    /// </summary>
    /// <param name="state">The current parsing state.</param>
    /// <param name="parseUntilOuterComma">The flag to stop parsing if top-level comma is encountered.</param>
    /// <returns>The root of parsed expression, if any; null otherwise.</returns>
    /// <remarks>Any NML expression can only contain ONE unit token. If if does, it is always the last token,
    /// and corresponding <see cref="UnitTerminatedExpression"/> node is the root of result.</remarks>
    public static ExpressionAST? TryParse(ref ParsingState state, bool parseUntilOuterComma = false)
    {
        var token = state.CurrentToken;
        if (token is null || (parseUntilOuterComma && token is BinaryOpToken { Type: OperatorType.Comma }))
            return null;

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

        for (token = state.NextToken; token is not null; token = state.CurrentToken)
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
                            return result;
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
                        case BinaryOperation { Precedence: > Grammar.TernaryOperatorPrecedence }:
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
                            if (current._parent is null) // For templates and realsprites usage, TODO process separately
                                return result;
                            state.AddUnexpected(token);
                            break;
                    }
                    break;

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
                            state.AddUnexpected(token);
                            break;

                        case BinaryOperation:
                        case ParentedExpression:
                        case FunctionCall:
                        case UnaryOperation:
                            if (current._parent is null)
                                return result;
                            state.AddUnexpected(token);
                            break;
                    }
                    break;

                case BinaryOpToken binaryOpToken:
                    switch (current)
                    {
                        case BinaryOperation binaryOp when binaryOpToken.Precedence > binaryOp.Precedence:
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
        return result;
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

    public abstract void VerifySyntax(ref readonly DiagnosticContext context);
}