using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal abstract partial class BaseSwitch : BaseStatementWithBlock
{
    private readonly IReadOnlyList<Line>? _content;
    private readonly IReadOnlyList<ReturnLine>? _returnLines;

    protected BaseSwitch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<Line> switchLines = new();
        List<ReturnLine> returnLines = new();
        var innerState = InnerState.ExpectingAnything;
        Line currentLine = new();
        for (var token = state.currentToken; token is not null;)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    switch (innerState)
                    {
                        case InnerState.ExpectingRangeColonSemicolon:
                            returnLines.Add(currentLine.ReturnLine);
                            break;

                        case InnerState.ExpectingColonSemicolon:
                            returnLines.Add(new ReturnLine(null, currentLine.Condition, null));
                            break;

                        case InnerState.ExpectingAfterRange:
                        case InnerState.ExpectingColon:
                        case InnerState.ExpectingValue:
                            switchLines.Add(currentLine);
                            break;

                        case InnerState.ExpectingAnything:
                            break;
                    }
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_Ending;

                case KeywordToken { Type: KeywordType.Return } returnKeyword:
                    switch (innerState)
                    {
                        case InnerState.ExpectingAnything:
                            state.Increment();
                        {
                            var expression = ExpressionAST.TryParse(state);
                            var semicolon = state.ExpectSemicolon();
                            returnLines.Add(new ReturnLine(returnKeyword, expression, semicolon));
                        }
                            innerState = InnerState.ExpectingAnything;
                            token = state.currentToken;
                            continue;

                        case InnerState.ExpectingValue:
                            state.IncrementSkippingComments();
                        {
                            var expression = ExpressionAST.TryParse(state);
                            var semicolon = state.ExpectSemicolon();
                            currentLine.ReturnLine = new ReturnLine(returnKeyword, expression, semicolon);
                        }
                            switchLines.Add(currentLine);
                            currentLine = new Line();
                            innerState = InnerState.ExpectingAnything;
                            token = state.currentToken;
                            continue;

                        default:
                            goto label_Unexpected;
                    }

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
                            currentLine.Condition = ExpressionAST.TryParse(state);
                            innerState = currentLine.Condition is UnitTerminatedExpression
                                ? InnerState.ExpectingColonSemicolon
                                : InnerState.ExpectingRangeColonSemicolon;
                            token = state.currentToken;
                            continue;

                        case InnerState.ExpectingAfterRange:
                            currentLine.ConditionRightSide = ExpressionAST.TryParse(state);
                            innerState = InnerState.ExpectingColon;
                            token = state.currentToken;
                            continue;

                        case InnerState.ExpectingValue:
                            currentLine.ReturnLine.Value = ExpressionAST.TryParse(state);
                            currentLine.ReturnLine.Semicolon = state.ExpectSemicolon();
                            switchLines.Add(currentLine);
                            currentLine = new Line();
                            innerState = InnerState.ExpectingAnything;
                            token = state.currentToken;
                            continue;

                        default:
                            goto label_Unexpected;
                    }

                case KeywordToken:
                    switch (innerState)
                    {
                        case InnerState.ExpectingAnything:
                            break;

                        case InnerState.ExpectingRangeColonSemicolon:
                            returnLines.Add(currentLine.ReturnLine);
                            break;

                        case InnerState.ExpectingColonSemicolon:
                            returnLines.Add(new ReturnLine(null, currentLine.Condition, null));
                            break;

                        case InnerState.ExpectingAfterRange:
                        case InnerState.ExpectingColon:
                        case InnerState.ExpectingValue:
                            switchLines.Add(currentLine);
                            break;
                    }
                    goto label_Ending;

                case ColonToken colonToken:
                    if (innerState == InnerState.ExpectingValue)
                    {
                        goto label_Unexpected;
                    }
                    currentLine.Colon = colonToken;
                    innerState = InnerState.ExpectingValue;
                    break;

                case RangeToken rangeToken:
                    if (innerState is not (InnerState.ExpectingRangeColonSemicolon or InnerState.ExpectingAnything))
                    {
                        goto label_Unexpected;
                    }
                    currentLine.Range = rangeToken;
                    innerState = InnerState.ExpectingAfterRange;
                    break;

                case SemicolonToken semicolonToken:
                    switch (innerState)
                    {
                        case InnerState.ExpectingRangeColonSemicolon:
                        case InnerState.ExpectingColonSemicolon:
                            returnLines.Add(new ReturnLine(null, currentLine.Condition, semicolonToken));
                            currentLine.Condition = null;
                            break;

                        case InnerState.ExpectingColon:
                        case InnerState.ExpectingValue:
                            currentLine.ReturnLine.Semicolon = semicolonToken;
                            switchLines.Add(currentLine);
                            currentLine = default;
                            break;

                        default:
                            goto label_Unexpected;
                    }
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
        _content = switchLines.ToMaybeList();
        _returnLines = returnLines.ToMaybeList();
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