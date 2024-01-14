using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statements.Models;

namespace NMLServer.Parsing.Statements;

internal sealed class SwitchParser : HeadingParser
{
    public static Switch Apply(KeywordToken alwaysSwitch)
    {
        Switch result = new();
        ParseHeading(alwaysSwitch, out result.Heading);

        while (result.OpeningBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' } opening:
                    result.OpeningBracket = opening;
                    break;

                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    Pointer++;
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        if (!areTokensLeft)
        {
            return result;
        }

        List<SwitchLine> lines = new(4);
        while (result.ClosingBracket is null && areTokensLeft)
        {
            var token = Tokens[Pointer];
            SwitchLine line = new();
            switch (token)
            {
                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    break;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    goto end;

                case KeywordToken { Type: KeywordType.Return } returnKeyword:
                {
                    ParseReturnStatement(ref line, returnKeyword);
                    lines.Add(line);
                    continue;
                }

                case RangeToken rangeToken:
                {
                    ParseFromRangeToken(ref line, rangeToken);
                    lines.Add(line);
                    continue;
                }

                case UnitToken:
                case BracketToken:
                case KeywordToken:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    lines.Add(ParseSingleLine());
                    continue;

                default:
                    UnexpectedTokens.Add(token);
                    break;
            }
            Pointer++;
        }

        end:
        result.Body = lines.MaybeToArray();
        return result;
    }

    private static SwitchLine ParseSingleLine()
    {
        SwitchLine result = new();

        while (result.Condition is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' }:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return result;

                case ColonToken colonToken:
                    ParseFromColonToken(ref result, colonToken);
                    return result;

                case KeywordToken { Type: KeywordType.Return } returnKeywordToken:
                    ParseReturnStatement(ref result, returnKeywordToken);
                    return result;

                case RangeToken rangeToken:
                    ParseFromRangeToken(ref result, rangeToken);
                    return result;

                case SemicolonToken semicolonToken:
                    result.Semicolon = semicolonToken;
                    Pointer++;
                    return result;

                case UnitToken:
                case BracketToken { Bracket: not '{' }:
                case KeywordToken:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    TryParseExpression(out result.Condition, out _);
                    continue;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case RangeToken rangeToken:
                    ParseFromRangeToken(ref result, rangeToken);
                    return result;

                case SemicolonToken semicolonToken:
                    result.Value = result.Condition;
                    result.Condition = null;
                    result.Semicolon = semicolonToken;
                    Pointer++;
                    return result;

                case ColonToken colonToken:
                    ParseFromColonToken(ref result, colonToken);
                    return result;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                case BracketToken { Bracket: '}' }:
                    return result;

                case KeywordToken { Type: KeywordType.Return } keywordToken:
                    ParseReturnStatement(ref result, keywordToken);
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        return result;
    }

    private static void ParseFromRangeToken(ref SwitchLine result, RangeToken rangeToken)
    {
        result.Range = rangeToken;
        Pointer++;

        while (result.ConditionRightSide is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                case BracketToken { Bracket: '}' }:
                    return;

                case BracketToken { Bracket: not '{' }:
                case KeywordToken { Type: not KeywordType.Return }:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    TryParseExpression(out result.ConditionRightSide, out _);
                    continue;

                case ColonToken colonToken:
                    ParseFromColonToken(ref result, colonToken);
                    return;

                case SemicolonToken semicolonToken:
                    UnexpectedTokens.Add(rangeToken);
                    result.Range = null;

                    result.Value = result.Condition;
                    result.Condition = null;

                    result.Semicolon = semicolonToken;
                    Pointer++;
                    return;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                case BracketToken { Bracket: '}' }:
                    return;

                case ColonToken colonToken:
                    ParseFromColonToken(ref result, colonToken);
                    return;

                case SemicolonToken semicolonToken:
                    result.Semicolon = semicolonToken;
                    Pointer++;
                    return;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
    }

    private static void ParseFromColonToken(ref SwitchLine result, ColonToken colonToken)
    {
        result.Colon = colonToken;
        Pointer++;

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case KeywordToken { Type: not KeywordType.Return, IsExpressionUsable: false }:
                case BracketToken { Bracket: '}' }:
                    return;

                case KeywordToken { Type: KeywordType.Return } tokenReturn:
                    ParseReturnStatement(ref result, tokenReturn);
                    return;

                case BracketToken { Bracket: not '{' }:
                case BaseValueToken:
                case UnitToken:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                    TryParseExpression(out result.Value, out var endingToken);
                    if (endingToken is not SemicolonToken semicolonToken)
                    {
                        result.Semicolon = TryParseSemicolon();
                        return;
                    }
                    result.Semicolon = semicolonToken;
                    Pointer++;
                    return;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
    }

    private static void ParseReturnStatement(ref SwitchLine result, KeywordToken alwaysReturn)
    {
        result.ReturnKeyword = alwaysReturn;
        Pointer++;

        while (result.Value is null && areTokensLeft)
        {
            var token = Tokens[Pointer];
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                case BracketToken { Bracket: not '{' }:
                case KeywordToken:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    TryParseExpression(out result.Value, out _);
                    continue;

                default:
                    UnexpectedTokens.Add(token);
                    break;
            }
            Pointer++;
        }
        while (result.Semicolon is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' }:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                case SemicolonToken semicolonToken:
                    result.Semicolon = semicolonToken;
                    break;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
    }
}