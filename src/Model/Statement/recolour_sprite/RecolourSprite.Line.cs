using NMLServer.Extensions;
using NMLServer.Model.Lexis;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Statement;

internal partial class RecolourSprite
{
    public readonly struct Line : IBlockContents<Line>
    {
        private readonly ExpressionRange lhs;
        private readonly ColonToken? _colon;
        private readonly ExpressionRange rhs;
        private readonly SemicolonToken? _semicolon;

        // -1 is unreachable but type system disallows lhs.End!
        public int End => _semicolon?.End ?? rhs.End ?? _colon?.End ?? lhs.End ?? -1;

        public static List<Line>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
        {
            List<Line> content = [];
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                        goto label_End;

                    case BracketToken { Bracket: '}' } expectedClosingBracket:
                        closingBracket = expectedClosingBracket;
                        state.Increment();
                        goto label_End;

                    case RangeToken:
                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                    case BracketToken { Bracket: not '{' }:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                        content.Add(new Line(ref state));
                        break;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
        label_End:
            return content.ToMaybeList();
        }

        private Line(ref ParsingState state)
        {
            lhs = new ExpressionRange(ref state);
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    case BracketToken { Bracket: '}' }:
                        return;

                    case RangeToken:
                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                    case BracketToken { Bracket: not '{' }:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                        if (_colon is null)
                            goto default;
                        rhs = new ExpressionRange(ref state);
                        _semicolon = state.ExpectSemicolon();
                        return;

                    case ColonToken colon when _colon is null:
                        _colon = colon;
                        state.Increment();
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
            lhs.Visualize(graph, n, ctx);
            _colon.MaybeVisualize(graph, n, ctx);
            rhs.Visualize(graph, n, ctx);
            _semicolon.MaybeVisualize(graph, n, ctx);
            return n;
        }
#endif
    }
}