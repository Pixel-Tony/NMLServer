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
    internal struct Line : IBlockContents<Line>
    {
        private readonly ExpressionRange _lhs;
        private readonly ColonToken? _colon;
        private readonly KeywordToken? _return;
        private ExpressionAST? _value;
        private SemicolonToken? _semicolon;

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

                    // TODO

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
        label_End:
            return contents.ToMaybeList();
        }



        public static List<Line>? ParseSomeInBlock(ref ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Line> content = [];
            var innerState = FSM.ExpectAny;
            Line current = new();
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' } bracket:
                        if (innerState is not FSM.ExpectAny)
                            content.Add(current);
                        closingBracket = bracket;
                        state.Increment();
                        goto label_Ending;

                    case KeywordToken { Type: KeywordType.Return } returnKeyword
                            when innerState is FSM.ExpectAny or FSM.ExpectValue:
                        current._returnKeyword = returnKeyword;
                        state.IncrementSkippingComments();
                        current._returnValue = ExpressionAST.TryParse(ref state);
                        current._semicolon = state.ExpectSemicolon();
                        content.Add(current);
                        current = new Line();
                        innerState = FSM.ExpectAny;
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
                                continue;

                            case FSM.ExpectAfterRange:
                                current._patternRightSide = ExpressionAST.TryParse(ref state);
                                innerState = FSM.ExpectColon;
                                continue;

                            case FSM.ExpectValue:
                                current._returnValue = ExpressionAST.TryParse(ref state);
                                current._semicolon = state.ExpectSemicolon();
                                content.Add(current);
                                current = new Line();
                                innerState = FSM.ExpectAny;
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
                        state.Increment();
                        innerState = FSM.ExpectValue;
                        break;

                    case RangeToken rangeToken
                            when innerState is FSM.ExpectAny or FSM.ExpectRangeColonSemicolon:
                        current._range = rangeToken;
                        state.Increment();
                        innerState = FSM.ExpectAfterRange;
                        break;

                    case SemicolonToken semicolonToken
                            when innerState is not (FSM.ExpectAny or FSM.ExpectAfterRange):
                        current._semicolon = semicolonToken;
                        state.Increment();
                        content.Add(current);
                        current = default;
                        innerState = FSM.ExpectAny;
                        break;

                    default:
                    label_Unexpected:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
        label_Ending:
            return content.ToMaybeList();
        }

        private Line(ref ParsingState state, ColonToken? colon = null, KeywordToken? returnKw = null)
        {
            _colon = colon;
            _return = returnKw;

            if (_colon is null & _return is null)
                _lhs = new ExpressionRange(ref state);

            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    // TODO
                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
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
            _lhs.Visualize(graph, n, context);
            _colon.MaybeVisualize(graph, n, context);
            _return.MaybeVisualize(graph, n, context);
            _value.MaybeVisualize(graph, n, context);
            _semicolon.MaybeVisualize(graph, n, context);
            return n;
        }
#endif
    }
}