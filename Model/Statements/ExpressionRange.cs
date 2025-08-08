using NMLServer.Model.Expressions;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Statements;

internal readonly struct ExpressionRange
{
    public readonly BaseExpression? lhs;
    public readonly RangeToken? range;
    public readonly BaseExpression? rhs;

    public int? End => rhs?.End ?? range?.End ?? lhs?.End;

    public ExpressionRange(BaseExpression? left, RangeToken? rangeToken, BaseExpression? right)
    {
        lhs = left;
        range = rangeToken;
        rhs = right;
    }

    public ExpressionRange(ref ParsingState state)
    {
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case ColonToken:
                case KeywordToken { IsExpressionUsable: false }:
                case BracketToken { Bracket: '}' }:
                case BinaryOpToken { Type: OperatorType.Comma }:
                case SemicolonToken:
                    return;

                case RangeToken foundRange when range is null:
                    range = foundRange;
                    state.Increment();
                    break;

                case KeywordToken:
                case BracketToken { Bracket: not '{' }:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    if (rhs is not null | (range is null & lhs is not null))
                        goto default;
                    var expr = BaseExpression.TryParse(ref state);
                    if (range is not null)
                    {
                        rhs = expr;
                        return;
                    }
                    lhs = expr;
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    }

    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "Line").WithStmtFeatures();
        lhs.MaybeVisualize(graph, n, ctx);
        range.MaybeVisualize(graph, n, ctx);
        rhs.MaybeVisualize(graph, n, ctx);
        return n;
    }
}