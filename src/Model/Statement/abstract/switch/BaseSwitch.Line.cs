using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class BaseSwitch
{
    internal record struct Line(ExpressionAST? Pattern, RangeToken? Range, ExpressionAST? PatternRightSide,
        ColonToken? Colon, KeywordToken? ReturnKeyword, ExpressionAST? ReturnValue,
        SemicolonToken? Semicolon) : IAllowsParseInsideBlock<Line>
    {
        public ExpressionAST? Pattern = Pattern;
        public RangeToken? Range = Range;
        public ExpressionAST? PatternRightSide = PatternRightSide;
        public ColonToken? Colon = Colon;
        public KeywordToken? ReturnKeyword = ReturnKeyword;
        public ExpressionAST? ReturnValue = ReturnValue;
        public SemicolonToken? Semicolon = Semicolon;

        public readonly int end => Semicolon?.end ?? (ReturnValue?.end ?? (ReturnKeyword?.end ?? (Colon?.end
            ?? (PatternRightSide?.end ?? (Range?.end ?? Pattern!.end)))));

        static List<Line>? IAllowsParseInsideBlock<Line>.ParseSomeInBlock(ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Line> content = new();
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
                        current.ReturnKeyword = returnKeyword;
                        state.IncrementSkippingComments();
                        current.ReturnValue = ExpressionAST.TryParse(state);
                        current.Semicolon = state.ExpectSemicolon();
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
                                current.Pattern = ExpressionAST.TryParse(state);
                                innerState = current.Pattern is UnitTerminatedExpression
                                    ? InnerState.ExpectingColonSemicolon
                                    : InnerState.ExpectingRangeColonSemicolon;
                                token = state.currentToken;
                                continue;

                            case InnerState.ExpectingAfterRange:
                                current.PatternRightSide = ExpressionAST.TryParse(state);
                                innerState = InnerState.ExpectingColon;
                                token = state.currentToken;
                                continue;

                            case InnerState.ExpectingValue:
                                current.ReturnValue = ExpressionAST.TryParse(state);
                                current.Semicolon = state.ExpectSemicolon();
                                content.Add(current);
                                current = new Line();
                                innerState = InnerState.ExpectingAnything;
                                token = state.currentToken;
                                continue;

                            default:
                                goto label_Unexpected;
                        }

                    case KeywordToken:
                        switch (innerState)
                        {
                            case InnerState.ExpectingRangeColonSemicolon:
                                content.Add(current);
                                goto label_Ending;

                            case InnerState.ExpectingColonSemicolon:
                                content.Add(current);
                                goto label_Ending;

                            case InnerState.ExpectingAfterRange:
                            case InnerState.ExpectingColon:
                            case InnerState.ExpectingValue:
                                content.Add(current);
                                goto label_Ending;

                            default:
                                goto label_Ending;
                        }

                    case ColonToken colonToken:
                        if (innerState is InnerState.ExpectingValue)
                        {
                            goto label_Unexpected;
                        }
                        current.Colon = colonToken;
                        innerState = InnerState.ExpectingValue;
                        break;

                    case RangeToken rangeToken:
                        if (innerState is InnerState.ExpectingAnything or InnerState.ExpectingRangeColonSemicolon)
                        {
                            current.Range = rangeToken;
                            innerState = InnerState.ExpectingAfterRange;
                            break;
                        }
                        goto label_Unexpected;

                    case SemicolonToken semicolonToken:
                        if (innerState is InnerState.ExpectingAnything or InnerState.ExpectingAfterRange)
                        {
                            goto label_Unexpected;
                        }
                        current.Semicolon = semicolonToken;
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