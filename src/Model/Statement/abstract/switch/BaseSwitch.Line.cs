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
    internal struct Line(
        ExpressionAST? pattern,
        RangeToken? range,
        ExpressionAST? patternRightSide,
        ColonToken? colon,
        KeywordToken? returnKeyword,
        ExpressionAST? returnValue,
        SemicolonToken? semicolon) : IBlockContents<Line>
    {
        private ExpressionAST? _pattern = pattern;
        private RangeToken? _range = range;
        private ExpressionAST? _patternRightSide = patternRightSide;
        private ColonToken? _colon = colon;
        private KeywordToken? _returnKeyword = returnKeyword;
        private ExpressionAST? _returnValue = returnValue;
        private SemicolonToken? _semicolon = semicolon;

        public readonly int End => _semicolon?.End ?? _returnValue?.End ?? _returnKeyword?.End ?? _colon?.End
            ?? _patternRightSide?.End ?? _range?.End ?? _pattern!.End;

        static List<Line>? IAllowsParseInsideBlock<Line>.ParseSomeInBlock(ref ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Line> content = [];
            var innerState = FSM.ExpectAny;
            Line current = new();
            for (var token = state.CurrentToken; token is not null;)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' } bracket:
                        if (innerState is not FSM.ExpectAny)
                            content.Add(current);
                        closingBracket = bracket;
                        state.Increment();
                        goto label_Ending;

                    case KeywordToken { Type: KeywordType.Return } returnKeyword:
                        if (innerState is not (FSM.ExpectAny or FSM.ExpectValue))
                            goto label_Unexpected;
                        current._returnKeyword = returnKeyword;
                        state.IncrementSkippingComments();
                        current._returnValue = ExpressionAST.TryParse(ref state);
                        current._semicolon = state.ExpectSemicolon();
                        content.Add(current);
                        current = new Line();
                        innerState = FSM.ExpectAny;
                        token = state.CurrentToken;
                        continue;

                    case UnitToken:
                    case BaseValueToken:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BracketToken { Bracket: not '{' }:
                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                        switch (innerState)
                        {
                            case FSM.ExpectAny:
                                current._pattern = ExpressionAST.TryParse(ref state);
                                innerState = current._pattern is UnitTerminatedExpression
                                    ? FSM.ExpectColonSemicolon
                                    : FSM.ExpectRangeColonSemicolon;
                                token = state.CurrentToken;
                                continue;

                            case FSM.ExpectAfterRange:
                                current._patternRightSide = ExpressionAST.TryParse(ref state);
                                innerState = FSM.ExpectColon;
                                token = state.CurrentToken;
                                continue;

                            case FSM.ExpectValue:
                                current._returnValue = ExpressionAST.TryParse(ref state);
                                current._semicolon = state.ExpectSemicolon();
                                content.Add(current);
                                current = new Line();
                                innerState = FSM.ExpectAny;
                                token = state.CurrentToken;
                                continue;

                            default:
                                goto label_Unexpected;
                        }

                    case KeywordToken:
                        if (innerState is not FSM.ExpectAny)
                            content.Add(current);
                        goto label_Ending;

                    case ColonToken colonToken:
                        if (innerState is FSM.ExpectValue)
                            goto label_Unexpected;
                        current._colon = colonToken;
                        innerState = FSM.ExpectValue;
                        break;

                    case RangeToken rangeToken:
                        if (innerState is FSM.ExpectAny or FSM.ExpectRangeColonSemicolon)
                        {
                            current._range = rangeToken;
                            innerState = FSM.ExpectAfterRange;
                            break;
                        }
                        goto label_Unexpected;

                    case SemicolonToken semicolonToken:
                        if (innerState is FSM.ExpectAny or FSM.ExpectAfterRange)
                            goto label_Unexpected;
                        current._semicolon = semicolonToken;
                        content.Add(current);
                        current = default;
                        innerState = FSM.ExpectAny;
                        break;

                    default:
                        label_Unexpected:
                        state.AddUnexpected(token);
                        break;
                }
                token = state.NextToken;
            }
            label_Ending:
            return content.ToMaybeList();
        }

        private enum FSM
        {
            ExpectAny, // Start of the line
            ExpectRangeColonSemicolon, // Expression present, can be a "key: value" or a "value;"
            ExpectAfterRange, // Expression and range operator present, expect second half of range
            ExpectColonSemicolon, // Expression +unit present
            ExpectColon, // Expression .. Expression (+unit) present, expect colon and value afterwards
            ExpectValue // Colon present, expect value
        }

#if TREE_VISUALIZER_ENABLED
        public readonly DotNode Visualize(DotGraph graph, DotNode parent, string context)
        {
            var n = VizExtensions.MakeNode(graph, parent, "Line");
            _pattern.MaybeVisualize(graph, n, context);
            _range.MaybeVisualize(graph, n, context);
            _patternRightSide.MaybeVisualize(graph, n, context);
            _colon.MaybeVisualize(graph, n, context);
            _returnKeyword?.Visualize(graph, n, context);
            _returnValue.MaybeVisualize(graph, n, context);
            _semicolon.MaybeVisualize(graph, n, context);
            return n;
        }
#endif
    }
}