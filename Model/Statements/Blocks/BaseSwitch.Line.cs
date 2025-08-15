using NMLServer.Extensions;
using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using DotNetGraph.Core;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Statements.Blocks;

internal partial class BaseSwitch
{
    internal readonly struct Line : IBlockContents<Line>
    {
        private readonly ExpressionRange _lhs;
        private readonly ColonToken? _colon;
        private readonly KeywordToken? _return;
        private readonly BaseExpression? _value;
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

                    case KeywordToken { IsDefiningStatement: true }:
                        goto label_End;

                    case KeywordToken { Keyword: Grammar.Keyword.Return } returnKw:
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
                    case KeywordToken { IsExpressionUsable: true }:
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
                    case KeywordToken { IsDefiningStatement: true }:
                        return;

                    case SemicolonToken semicolon:
                        _semicolon = semicolon;
                        state.Increment();
                        return;

                    case KeywordToken { Keyword: Grammar.Keyword.Return } returnKeyword:
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
                    case KeywordToken { IsExpressionUsable: true }:
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
            _value = BaseExpression.TryParse(ref state);
            _semicolon = state.ExpectSemicolon();
        }

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
    }
}