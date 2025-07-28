#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif
using NMLServer.Extensions;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class BaseSwitch
{
    internal readonly struct Line : IBlockContents<Line>
    {
        private readonly ExpressionRange _lhs;
        private readonly ColonToken? _colon;
        private readonly KeywordToken? _return;
        private readonly ExpressionAST? _value;
        private readonly SemicolonToken? _semicolon;

        // -1 is unreachable but type system disallows lhs.End!
        public readonly int End => _semicolon?.End ?? _value?.End ?? _return?.End ?? _colon?.End ?? _lhs.End ?? -1;

        public static List<Line>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
        {
            List<Line> contents = [];
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' } closing:
                        closingBracket = closing;
                        state.Increment();
                        goto label_End;

                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                        goto label_End;

                    case KeywordToken { Type: KeywordType.Return } returnKw:
                        contents.Add(new Line(ref state, null, returnKw));
                        break;

                    case ColonToken colon:
                        contents.Add(new Line(ref state, colon));
                        break;

                    case UnitToken:
                    case RangeToken:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                    case BracketToken { Bracket: not '{' }:
                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                        contents.Add(new Line(ref state));
                        break;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
        label_End:
            return contents.ToMaybeList();
        }

        private Line(ref ParsingState state, ColonToken? colon = null, KeywordToken? returnKw = null)
        {
            _colon = colon;
            _return = returnKw;
            if (_return is not null)
            {
                state.Increment();
                goto label_AfterReturn;
            }
            if (_colon is not null)
                state.Increment();
            else
                _lhs = new ExpressionRange(ref state);

            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' }:
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                        return;

                    case SemicolonToken semicolon:
                        _semicolon = semicolon;
                        state.Increment();
                        return;

                    case KeywordToken { Type: KeywordType.Return } returnKeyword:
                        if (_colon is null)
                            goto default;
                        _return = returnKeyword;
                        state.Increment();
                        goto label_AfterReturn;

                    case ColonToken colonToken when _colon is null:
                        _colon = colonToken;
                        state.Increment();
                        break;

                    case UnitToken:
                    case RangeToken:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                    case BracketToken { Bracket: not '{' }:
                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                        if (_colon is null)
                            goto default;
                        goto label_AfterReturn;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
            return;
        label_AfterReturn:
            _value = ExpressionAST.TryParse(ref state);
            _semicolon = state.ExpectSemicolon();
        }

#if TREE_VISUALIZER_ENABLED
        public readonly DotNode Visualize(DotGraph graph, DotNode parent, string context)
        {
            var n = VizExtensions.MakeNode(graph, parent, "Line");
            _lhs.Visualize(graph, n, context);
            _colon.MaybeVisualize(graph, n, context);
            _return?.Visualize(graph, n, context);
            _value.MaybeVisualize(graph, n, context);
            _semicolon.MaybeVisualize(graph, n, context);
            return n;
        }
#endif
    }
}