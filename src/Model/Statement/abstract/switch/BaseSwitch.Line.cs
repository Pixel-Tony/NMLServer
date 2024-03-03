using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class BaseSwitch
{
    internal struct Line(ExpressionAST? pattern, RangeToken? range, ExpressionAST? patternRightSide,
        ColonToken? colon, KeywordToken? returnKeyword, ExpressionAST? returnValue,
        SemicolonToken? semicolon) : IAllowsParseInsideBlock<Line>
    {
        private ExpressionAST? _pattern = pattern;
        private RangeToken? _range = range;
        private ExpressionAST? _patternRightSide = patternRightSide;
        private ColonToken? _colon = colon;
        private KeywordToken? _returnKeyword = returnKeyword;
        private ExpressionAST? _returnValue = returnValue;
        private SemicolonToken? _semicolon = semicolon;

        public readonly int end => _semicolon?.end ?? (_returnValue?.end ?? (_returnKeyword?.end ?? (_colon?.end
            ?? (_patternRightSide?.end ?? (_range?.end ?? _pattern!.end)))));

        static List<Line>? IAllowsParseInsideBlock<Line>.ParseSomeInBlock(ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Line> content = [];
            var innerState = InnerState.ExpectingAnything;
            Line current = new Line();
            for (var token = state.currentToken; token is not null;)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' } expectedClosingBracket:
                        if (innerState is not InnerState.ExpectingAnything)
                        {
                            content.Add(current);
                        }
                        closingBracket = expectedClosingBracket;
                        state.Increment();
                        goto label_Ending;

                    case KeywordToken { Type: KeywordType.Return } returnKeyword:
                        if (innerState is not (InnerState.ExpectingAnything or InnerState.ExpectingValue))
                        {
                            goto label_Unexpected;
                        }
                        current._returnKeyword = returnKeyword;
                        state.IncrementSkippingComments();
                        current._returnValue = ExpressionAST.TryParse(state);
                        current._semicolon = state.ExpectSemicolon();
                        content.Add(current);
                        current = new Line();
                        innerState = InnerState.ExpectingAnything;
                        token = state.currentToken;
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
                            case InnerState.ExpectingAnything:
                                current._pattern = ExpressionAST.TryParse(state);
                                innerState = current._pattern is UnitTerminatedExpression
                                    ? InnerState.ExpectingColonSemicolon
                                    : InnerState.ExpectingRangeColonSemicolon;
                                token = state.currentToken;
                                continue;

                            case InnerState.ExpectingAfterRange:
                                current._patternRightSide = ExpressionAST.TryParse(state);
                                innerState = InnerState.ExpectingColon;
                                token = state.currentToken;
                                continue;

                            case InnerState.ExpectingValue:
                                current._returnValue = ExpressionAST.TryParse(state);
                                current._semicolon = state.ExpectSemicolon();
                                content.Add(current);
                                current = new Line();
                                innerState = InnerState.ExpectingAnything;
                                token = state.currentToken;
                                continue;

                            default:
                                goto label_Unexpected;
                        }

                    case KeywordToken:
                        if (innerState is not InnerState.ExpectingAnything)
                        {
                            content.Add(current);
                        }
                        goto label_Ending;

                    case ColonToken colonToken:
                        if (innerState is InnerState.ExpectingValue)
                        {
                            goto label_Unexpected;
                        }
                        current._colon = colonToken;
                        innerState = InnerState.ExpectingValue;
                        break;

                    case RangeToken rangeToken:
                        if (innerState is InnerState.ExpectingAnything or InnerState.ExpectingRangeColonSemicolon)
                        {
                            current._range = rangeToken;
                            innerState = InnerState.ExpectingAfterRange;
                            break;
                        }
                        goto label_Unexpected;

                    case SemicolonToken semicolonToken:
                        if (innerState is InnerState.ExpectingAnything or InnerState.ExpectingAfterRange)
                        {
                            goto label_Unexpected;
                        }
                        current._semicolon = semicolonToken;
                        content.Add(current);
                        current = default;
                        innerState = InnerState.ExpectingAnything;
                        break;

                    default:
                        label_Unexpected:
                        state.AddUnexpected(token);
                        break;
                }
                token = state.nextToken;
            }
            label_Ending:
            return content.ToMaybeList();
        }

        private enum InnerState
        {
            ExpectingAnything, // Start of the line
            ExpectingRangeColonSemicolon, // Expression present, can be a "key: value" or a "value;"
            ExpectingAfterRange, // Expression and range operator present, expect second half of range
            ExpectingColonSemicolon, // Expression +unit present
            ExpectingColon, // Expression .. Expression (+unit) present, expect colon and value afterwards
            ExpectingValue // Colon present, expect value
        }
    }
}