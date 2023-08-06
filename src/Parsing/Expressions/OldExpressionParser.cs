using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class OldExpressionParser
{
    private readonly IEnumerator<Token> _tokens;

    public OldExpressionParser(IEnumerator<Token> tokens)
    {
        _tokens = tokens;
    }

    public (ExpressionAST?, Token?) ParseExpression()
    {
        ExpressionAST? tree = null;

        while (_tokens.MoveNext())
        {
            Token current = _tokens.Current;
            switch (current)
            {
                case BinaryOpToken binaryOpToken:
                {
                    switch (tree)
                    {
                        case BinaryOperation treeAsBinaryOperation:
                            var operatorPrecedence = binaryOpToken.precedence;
                            for (var subTree = treeAsBinaryOperation.Right;
                                 subTree is BinaryOperation subTreeAsBinary;
                                 subTree = subTreeAsBinary.Right)
                            {
                                if (subTreeAsBinary.Operation.precedence < operatorPrecedence)
                                {
                                    continue;
                                }
                                var result = CompleteBinaryOperation(subTreeAsBinary.Right, binaryOpToken);
                                (subTreeAsBinary.Right, var ender) = result;
                                return (tree, ender);
                            }
                            return CompleteBinaryOperation(tree, binaryOpToken);

                        case TernaryOperation ternaryOperation:
                            if (binaryOpToken.Operation == ",")
                            {
                                return CompleteBinaryOperation(tree, binaryOpToken);
                            }
                            for (var subnode = ternaryOperation.FalseBranch;
                                 subnode is TernaryOperation subnodeAsTernary;
                                 subnode = subnodeAsTernary.TrueBranch)
                            {
                                if (subnodeAsTernary.TrueBranch is TernaryOperation)
                                {
                                    continue;
                                }
                                var result = CompleteBinaryOperation(subnodeAsTernary.FalseBranch, binaryOpToken);
                                (subnodeAsTernary.FalseBranch, var ending) = result;
                                return (tree, ending);
                            }
                            return CompleteBinaryOperation(tree, binaryOpToken);
                        case null:
                        case FunctionCall:
                        case ParentedExpression:
                        case ValueNode:
                            return CompleteBinaryOperation(tree, binaryOpToken);
                        default:
                            throw new ArgumentOutOfRangeException(tree.ToString());
                    }
                }
                case UnaryOpToken unary when tree is null:
                {
                    var (subExpression, ender) = ParseExpression();
                    return (new UnaryOperation(unary, subExpression), ender);
                }
                case UnaryOpToken:
                    return (tree, current);
                case BracketToken bracket:
                    return (tree, current);
                    // switch (bracket.Bracket)
                    // {
                    //     case BracketType.CurlyOpening:
                    //     case BracketType.CurlyClosing:
                    //         return (tree, bracket);
                    //     case BracketType.RoundOpening when tree is null:
                    //         throw new NotImplementedException();
                    //         var (parentedExpr, roundEnder) = ParseExpression();
                    //     {
                    //         return roundEnder is BracketToken { Bracket: BracketType.RoundClosing } roundClosing
                    //             ? (new ParentedExpression((bracket, roundClosing), parentedExpr), null)
                    //             : (new ParentedExpression((bracket, null), parentedExpr), ender: roundEnder);
                    //     }
                    //     case BracketType.RoundOpening:
                    //         throw new NotImplementedException();
                    //         /* tree is not null */
                    //         var subNode = tree;
                    //         while (subNode is not Variable or null)
                    //         {
                    //             switch (subNode)
                    //             {
                    //                 case FunctionCall:
                    //                 case LiteralString:
                    //                 case Number:
                    //                 case ValueNode:
                    //                     return (tree, current);
                    //                 case TernaryOperation ternaryOperation:
                    //                     if (ternaryOperation.FalseBranch is not Variable or null)
                    //                     {
                    //                         subNode = ternaryOperation.FalseBranch;
                    //                         continue;
                    //                     }
                    //                     if (ternaryOperation.FalseBranch is null)
                    //                     {
                    //                         return (tree, current);
                    //                     }
                    //                     var (result, ender) = ParseExpression();
                    //                 {
                    //                     if (ender is BracketToken { Bracket: BracketType.RoundClosing } roundClosing)
                    //                     {
                    //                         ternaryOperation.FalseBranch = new FunctionCall(
                    //                             (ternaryOperation.FalseBranch as Variable)!.Value,
                    //                             result,
                    //                             (bracket, roundClosing)
                    //                         );
                    //                         return (tree, null);
                    //                     }
                    //                     ternaryOperation.FalseBranch = new FunctionCall(
                    //                         (ternaryOperation.FalseBranch as Variable)!.Value,
                    //                         result,
                    //                         (bracket, null)
                    //                     );
                    //                     return (tree, ender);
                    //                 }
                    //                 case UnaryOperation unaryOperation:
                    //                     subNode = unaryOperation.Expression;
                    //                     continue;
                    //                 default:
                    //                     throw new ArgumentOutOfRangeException(nameof(subNode));
                    //             }
                    //         }
                    //         if (subNode is null)
                    //         {
                    //             return (tree, current);
                    //         }
                    //         throw new NotImplementedException();
                    //     case BracketType.RoundClosing:
                    //         return (tree, current);
                    //     case BracketType.SquareOpening when tree is not null:
                    //         return (tree, current);
                    //     case BracketType.SquareOpening:
                    //     {
                    //         var (list, ender) = ParseExpression();
                    //         return ender is not BracketToken { Bracket: BracketType.SquareClosing } closingBracket
                    //             ? (new ArrayExpression(bracket, list, null), ender)
                    //             : (new ArrayExpression(bracket, list, closingBracket), null);
                    //     }
                    //     case BracketType.SquareClosing:
                    //         return (tree, current);
                    //     default:
                    //         throw new ArgumentOutOfRangeException();
                    // }
                case LiteralToken literal when tree is null:
                    tree = new Variable(literal);
                    continue;
                // Presence of this token means...
                // ...the end of the expression...
                case ColonToken:
                case SemicolonToken:
                // ...or the invalid token sequence ahead
                // (LiteralToken when tree is not null -> error)
                case LiteralToken:
                case FailedToken:
                case FeatureToken:
                case KeywordToken:
                    return (tree, current);
                case TernaryOpToken questionMark:
                    for (var notCommaNode = tree;
                         notCommaNode is BinaryOperation { Operation.Operation: "," } commas;
                         notCommaNode = commas.Right)
                    {
                        if (commas.Right is BinaryOperation { Operation.Operation: "," })
                        {
                            continue;
                        }
                        (commas.Right, var ending) = CompleteTernaryOperation(commas.Right, questionMark);
                        return (tree, ending);
                    }
                    return CompleteTernaryOperation(tree, questionMark);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        // EOF, return as is
        return (tree, null);
    }

    private (ExpressionAST?, Token?) CompleteBinaryOperation(ExpressionAST? left, BinaryOpToken opToken)
    {
        var (right, exprEnder) = ParseExpression();
        var tree = new BinaryOperation(left, opToken, right);
        return (tree, exprEnder);
    }

    private (ExpressionAST?, Token?) CompleteTernaryOperation(ExpressionAST? condition, TernaryOpToken questionMark)
    {
        var (truthy, truthyEnder) = ParseExpression();
        switch (truthyEnder)
        {
            case ColonToken colon:
                var (falsy, endingToken) = ParseExpression();
                return (new TernaryOperation(condition, truthy, falsy, questionMark, colon), endingToken);
            default:
                return (new TernaryOperation(condition, truthy, null, questionMark, null), truthyEnder);
        }
    }
}