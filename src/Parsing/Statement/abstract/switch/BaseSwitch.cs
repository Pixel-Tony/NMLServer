using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseSwitch : BaseStatementWithBlock
{
    public readonly SwitchLine[]? Content;
    public readonly SwitchReturnLine[]? ReturnLines;

    protected BaseSwitch(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }

        List<SwitchLine> switchLines = new();
        List<SwitchReturnLine> returnLines = new();

        var innerState = ParseFSM.ExpectsAnything;
        SwitchLine currentLine = new();
        for (var token = state.currentToken; token is not null;)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    switch (innerState)
                    {
                        case ParseFSM.ExpectsRangeColonSemicolon:
                            returnLines.Add(currentLine.ReturnLine);
                            break;

                        case ParseFSM.ExpectsColonSemicolon:
                            returnLines.Add(new SwitchReturnLine(null, currentLine.Condition, null));
                            break;

                        case ParseFSM.ExpectsAfterRange:
                        case ParseFSM.ExpectsColon:
                        case ParseFSM.ExpectsValue:
                            switchLines.Add(currentLine);
                            break;

                        case ParseFSM.ExpectsAnything:
                            break;
                    }
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_Ending;

                case KeywordToken { Type: KeywordType.Return } returnKeyword:
                    switch (innerState)
                    {
                        case ParseFSM.ExpectsAnything:
                            state.Increment();
                            returnLines.Add(new SwitchReturnLine(
                                    returnKeyword,
                                    ExpressionAST.TryParse(state),
                                    state.ExpectSemicolon()
                                )
                            );
                            innerState = ParseFSM.ExpectsAnything;
                            token = state.currentToken;
                            continue;

                        case ParseFSM.ExpectsValue:
                            currentLine.ReturnLine.ReturnKeyword = returnKeyword;
                            state.Increment();

                            currentLine.ReturnLine.Value = ExpressionAST.TryParse(state);
                            currentLine.ReturnLine.Semicolon = state.ExpectSemicolon();
                            switchLines.Add(currentLine);
                            currentLine = new SwitchLine();
                            innerState = ParseFSM.ExpectsAnything;
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
                        case ParseFSM.ExpectsAnything:
                            currentLine.Condition = ExpressionAST.TryParse(state);
                            innerState = currentLine.Condition is UnitTerminatedExpression
                                ? ParseFSM.ExpectsColonSemicolon
                                : ParseFSM.ExpectsRangeColonSemicolon;
                            token = state.currentToken;
                            continue;

                        case ParseFSM.ExpectsAfterRange:
                            currentLine.ConditionRightSide = ExpressionAST.TryParse(state);
                            innerState = ParseFSM.ExpectsColon;
                            token = state.currentToken;
                            continue;

                        case ParseFSM.ExpectsValue:
                            currentLine.ReturnLine.Value = ExpressionAST.TryParse(state);
                            currentLine.ReturnLine.Semicolon = state.ExpectSemicolon();
                            switchLines.Add(currentLine);
                            currentLine = new SwitchLine();
                            innerState = ParseFSM.ExpectsAnything;
                            token = state.currentToken;
                            continue;

                        default:
                            goto label_Unexpected;
                    }

                case KeywordToken:
                    switch (innerState)
                    {
                        case ParseFSM.ExpectsAnything:
                            break;

                        case ParseFSM.ExpectsRangeColonSemicolon:
                            returnLines.Add(currentLine.ReturnLine);
                            break;

                        case ParseFSM.ExpectsColonSemicolon:
                            returnLines.Add(new SwitchReturnLine(null, currentLine.Condition, null));
                            break;

                        case ParseFSM.ExpectsAfterRange:
                        case ParseFSM.ExpectsColon:
                        case ParseFSM.ExpectsValue:
                            switchLines.Add(currentLine);
                            break;
                    }
                    goto label_Ending;

                case ColonToken colonToken:
                    if (innerState == ParseFSM.ExpectsValue)
                    {
                        goto label_Unexpected;
                    }
                    currentLine.Colon = colonToken;
                    innerState = ParseFSM.ExpectsValue;
                    break;

                case RangeToken rangeToken:
                    if (innerState == ParseFSM.ExpectsRangeColonSemicolon || innerState == ParseFSM.ExpectsAnything)
                    {
                        currentLine.Range = rangeToken;
                        innerState = ParseFSM.ExpectsAfterRange;
                        break;
                    }
                    goto label_Unexpected;

                case SemicolonToken semicolonToken:
                    switch (innerState)
                    {
                        case ParseFSM.ExpectsRangeColonSemicolon:
                        case ParseFSM.ExpectsColonSemicolon:
                            returnLines.Add(new SwitchReturnLine(null, currentLine.Condition, semicolonToken));
                            currentLine.Condition = null;
                            break;

                        case ParseFSM.ExpectsColon:
                        case ParseFSM.ExpectsValue:
                            currentLine.ReturnLine.Semicolon = semicolonToken;
                            switchLines.Add(currentLine);
                            currentLine = default;
                            break;

                        default:
                            goto label_Unexpected;
                    }
                    innerState = ParseFSM.ExpectsAnything;
                    break;

                default:
                    label_Unexpected:
                    state.AddUnexpected(token);
                    break;
            }
            token = state.nextToken;
        }

        label_Ending:
        Content = switchLines.ToArrayOrNull();
        ReturnLines = returnLines.ToArrayOrNull();
    }

    private enum ParseFSM
    {
        // Start of the line
        ExpectsAnything,

        // Expression present, can be a "key: value" or a "value;"
        ExpectsRangeColonSemicolon,

        // Expression and range operator present, expect second half of range
        ExpectsAfterRange,

        // Expression +unit present
        ExpectsColonSemicolon,

        // Expression .. Expression (+unit) present, expect colon and value afterwards
        ExpectsColon,

        // Colon present, expect value
        ExpectsValue
    }
}