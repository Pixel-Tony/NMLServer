using NMLServer.Model.Expression;
using NMLServer.Model.Lexis;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Statement;

internal readonly struct ExpressionRange
{
    public readonly ExpressionAST? lhs;
    public readonly RangeToken? range;
    public readonly ExpressionAST? rhs;

    public int? End => rhs?.End ?? range?.End ?? lhs?.End;

    public ExpressionRange(ExpressionAST? left, RangeToken? rangeToken, ExpressionAST? right)
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
                case KeywordToken { Kind: not KeywordKind.ExpressionUsable }:
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
                    var expr = ExpressionAST.TryParse(ref state);
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

#if TREE_VISUALIZER_ENABLED
    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "Line").WithStmtFeatures();
        lhs.MaybeVisualize(graph, n, ctx);
        range.MaybeVisualize(graph, n, ctx);
        rhs.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}