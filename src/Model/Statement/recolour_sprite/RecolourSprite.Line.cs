using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class RecolourSprite
{
    public readonly struct Line : IAllowsParseInsideBlock<Line>
    {
        private readonly ExpressionAST _leftLeft;
        private readonly RangeToken? _leftRange;
        private readonly ExpressionAST? _leftRight;
        private readonly ColonToken? _colon;
        private readonly ExpressionAST? _rightLeft;
        private readonly RangeToken? _rightRange;
        private readonly ExpressionAST? _rightRight;
        private readonly SemicolonToken? _semicolon;

        public int End => _semicolon?.End ?? _rightRight?.End ?? _rightRange?.End ?? _rightLeft?.End ?? _colon?.End
            ?? _leftRight?.End ?? _leftRange?.End ?? _leftLeft.End;

        static List<Line>? IAllowsParseInsideBlock<Line>.ParseSomeInBlock(ref ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Line> content = [];
            for (var token = state.CurrentToken; token is not null; token = state.CurrentToken)
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
            ExpressionOrRange(ref state, ref _leftLeft!, ref _leftRange, ref _leftRight, true);
            for (var token = state.CurrentToken; token is not null; token = state.NextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    case BracketToken { Bracket: '}' }:
                        return;

                    case ColonToken colonToken:
                        _colon = colonToken;
                        state.Increment();
                        goto label_Right;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            return;

        label_Right:
            ExpressionOrRange(ref state, ref _rightLeft, ref _rightRange, ref _rightRight, false);
            for (var token = state.CurrentToken; token is not null; token = state.NextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    case BracketToken { Bracket: '}' }:
                        return;

                    case SemicolonToken semicolonToken:
                        _semicolon = semicolonToken;
                        state.Increment();
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        }

        private static void ExpressionOrRange(ref ParsingState state, ref ExpressionAST? left, ref RangeToken? range,
            ref ExpressionAST? right, bool stopAtColon)
        {
            for (var token = state.CurrentToken; token is not null; token = state.NextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    case BracketToken { Bracket: '}' }:
                        return;

                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                    case BracketToken { Bracket: not '{' }:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                        left = ExpressionAST.TryParse(ref state);
                        goto label_Range;

                    case RangeToken rangeToken:
                        range = rangeToken;
                        state.Increment();
                        goto label_Right;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            return;

        label_Range:
            for (var token = state.CurrentToken; token is not null; token = state.NextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    case BracketToken { Bracket: '}' }:
                    case ColonToken when stopAtColon:
                    case SemicolonToken when !stopAtColon:
                        return;

                    case RangeToken rangeToken:
                        range = rangeToken;
                        state.Increment();
                        goto label_Right;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            return;

        label_Right:
            for (var token = state.CurrentToken; token is not null; token = state.NextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    case BracketToken { Bracket: '}' }:
                    case ColonToken when stopAtColon:
                    case SemicolonToken when !stopAtColon:
                        return;

                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                    case BracketToken { Bracket: not '{' }:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                        right = ExpressionAST.TryParse(ref state);
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        }

        public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        {
            var n = VizExtensions.MakeNode(graph, parent, "Line").WithStmtFeatures();
            _leftLeft.Visualize(graph, n, ctx);
            _leftRange.MaybeVisualize(graph, n, ctx);
            _leftRight.MaybeVisualize(graph, n, ctx);
            _colon.MaybeVisualize(graph, n, ctx);
            _rightLeft.MaybeVisualize(graph, n, ctx);
            _rightRange.MaybeVisualize(graph, n, ctx);
            _rightRight.MaybeVisualize(graph, n, ctx);
            _semicolon.MaybeVisualize(graph, n, ctx);
            return n;
        }
    }
}