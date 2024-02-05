using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseSwitch : BaseStatementWithBlock
{
    public readonly IReadOnlyList<SwitchLine>? Content;
    public readonly IReadOnlyList<SwitchReturnLine>? ReturnLines;

    protected BaseSwitch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<SwitchLine> switchLines = new();
        List<SwitchReturnLine> returnLines = new();
        var innerState = InnerState.ExpectAnything;
        SwitchLine currentLine = new();
        for (var token = state.currentToken; token is not null;)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    switch (innerState)
                    {
                        case InnerState.ExpectRangeColonSemicolon:
                            returnLines.Add(currentLine.ReturnLine);
                            break;

                        case InnerState.ExpectColonSemicolon:
                            returnLines.Add(new SwitchReturnLine(null, currentLine.Condition, null));
                            break;

                        case InnerState.ExpectAfterRange:
                        case InnerState.ExpectColon:
                        case InnerState.ExpectValue:
                            switchLines.Add(currentLine);
                            break;

                        case InnerState.ExpectAnything:
                            break;
                    }
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_Ending;

                case KeywordToken { Type: KeywordType.Return } returnKeyword:
                    switch (innerState)
                    {
                        case InnerState.ExpectAnything:
                            state.Increment();
                            returnLines.Add(new SwitchReturnLine(
                                    returnKeyword,
                                    ExpressionAST.TryParse(state),
                                    state.ExpectSemicolon()
                                )
                            );
                            innerState = InnerState.ExpectAnything;
                            token = state.currentToken;
                            continue;

                        case InnerState.ExpectValue:
                            currentLine.ReturnLine.ReturnKeyword = returnKeyword;
                            state.Increment();

                            currentLine.ReturnLine.Value = ExpressionAST.TryParse(state);
                            currentLine.ReturnLine.Semicolon = state.ExpectSemicolon();
                            switchLines.Add(currentLine);
                            currentLine = new SwitchLine();
                            innerState = InnerState.ExpectAnything;
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
                        case InnerState.ExpectAnything:
                            currentLine.Condition = ExpressionAST.TryParse(state);
                            innerState = currentLine.Condition is UnitTerminatedExpression
                                ? InnerState.ExpectColonSemicolon
                                : InnerState.ExpectRangeColonSemicolon;
                            token = state.currentToken;
                            continue;

                        case InnerState.ExpectAfterRange:
                            currentLine.ConditionRightSide = ExpressionAST.TryParse(state);
                            innerState = InnerState.ExpectColon;
                            token = state.currentToken;
                            continue;

                        case InnerState.ExpectValue:
                            currentLine.ReturnLine.Value = ExpressionAST.TryParse(state);
                            currentLine.ReturnLine.Semicolon = state.ExpectSemicolon();
                            switchLines.Add(currentLine);
                            currentLine = new SwitchLine();
                            innerState = InnerState.ExpectAnything;
                            token = state.currentToken;
                            continue;

                        default:
                            goto label_Unexpected;
                    }

                case KeywordToken:
                    switch (innerState)
                    {
                        case InnerState.ExpectAnything:
                            break;

                        case InnerState.ExpectRangeColonSemicolon:
                            returnLines.Add(currentLine.ReturnLine);
                            break;

                        case InnerState.ExpectColonSemicolon:
                            returnLines.Add(new SwitchReturnLine(null, currentLine.Condition, null));
                            break;

                        case InnerState.ExpectAfterRange:
                        case InnerState.ExpectColon:
                        case InnerState.ExpectValue:
                            switchLines.Add(currentLine);
                            break;
                    }
                    goto label_Ending;

                case ColonToken colonToken:
                    if (innerState == InnerState.ExpectValue)
                    {
                        goto label_Unexpected;
                    }
                    currentLine.Colon = colonToken;
                    innerState = InnerState.ExpectValue;
                    break;

                case RangeToken rangeToken:
                    if (innerState is not (InnerState.ExpectRangeColonSemicolon or InnerState.ExpectAnything))
                    {
                        goto label_Unexpected;
                    }
                    currentLine.Range = rangeToken;
                    innerState = InnerState.ExpectAfterRange;
                    break;

                case SemicolonToken semicolonToken:
                    switch (innerState)
                    {
                        case InnerState.ExpectRangeColonSemicolon:
                        case InnerState.ExpectColonSemicolon:
                            returnLines.Add(new SwitchReturnLine(null, currentLine.Condition, semicolonToken));
                            currentLine.Condition = null;
                            break;

                        case InnerState.ExpectColon:
                        case InnerState.ExpectValue:
                            currentLine.ReturnLine.Semicolon = semicolonToken;
                            switchLines.Add(currentLine);
                            currentLine = default;
                            break;

                        default:
                            goto label_Unexpected;
                    }
                    innerState = InnerState.ExpectAnything;
                    break;

                default:
                    label_Unexpected:
                    state.AddUnexpected(token);
                    break;
            }
            token = state.nextToken;
        }
        label_Ending:
        Content = switchLines.ToMaybeList();
        ReturnLines = returnLines.ToMaybeList();
    }

    private enum InnerState
    {
        ExpectAnything,            // Start of the line
        ExpectRangeColonSemicolon, // Expression present, can be a "key: value" or a "value;"
        ExpectAfterRange,          // Expression and range operator present, expect second half of range
        ExpectColonSemicolon,      // Expression +unit present
        ExpectColon,               // Expression .. Expression (+unit) present, expect colon and value afterwards
        ExpectValue                // Colon present, expect value
    }
}