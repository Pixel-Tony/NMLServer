using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;
using NMLServer.Parsing.Statement.Blocks;

namespace NMLServer.Parsing.Statement;

internal class TracktypeTableParser : AttributeParser
{
    public static TracktypeTable Apply(KeywordToken keyword)
    {
        TracktypeTable result = new(keyword);
        Pointer++;

        while (result.OpeningBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    Pointer++;
                    return result;

                case BracketToken { Bracket: '{' } opening:
                    result.OpeningBracket = opening;
                    break;

                case KeywordToken:
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

        List<(BaseValueToken? identifier, BinaryOpToken? comma)> standaloneLabels = new();
        List<TrackTypeFallbackEntry> fallbackLabels = new();
        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            BaseValueToken? key = null;
            bool canExpectFallbacks = true;
            switch (current)
            {
                case IdentifierToken identifier:
                    key = identifier;
                    Pointer++;
                    break;

                case StringToken stringToken:
                    key = stringToken;
                    Pointer++;
                    canExpectFallbacks = false;
                    break;

                case ColonToken:
                    break;

                case BracketToken {Bracket: '}'} closingBracket:
                    result.ClosingBracket = closingBracket;
                    Pointer++;
                    return result;

                case KeywordToken:
                    goto end;

                default:
                    UnexpectedTokens.Add(current);
                    Pointer++;
                    continue;
            }

            if (Pointer >= Max)
            {
                continue;
            }

            current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closing:
                    result.ClosingBracket = closing;
                    Pointer++;
                    goto end;

                case BinaryOpToken { Type: OperatorType.Comma } comma:
                    standaloneLabels.Add((key, comma));
                    Pointer++;
                    break;

                case ColonToken colonToken when canExpectFallbacks:
                    Pointer++;
                    var entry = ParseFallbackEntry(key, colonToken);
                    fallbackLabels.Add(entry);
                    continue;

                default:
                    UnexpectedTokens.Add(current);
                    Pointer++;
                    break;
            }
        }
        end:
        if (standaloneLabels.Count > 0)
        {
            result.StandaloneLabels = standaloneLabels.ToArray();
        }
        if (fallbackLabels.Count > 0)
        {
            result.FallbackLabels = fallbackLabels.ToArray();
        }
        return result;
    }

    private static TrackTypeFallbackEntry ParseFallbackEntry(BaseValueToken? key, ColonToken? colonToken)
    {
        TrackTypeFallbackEntry result = new(key, colonToken);

        while (result.Fallback is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '[' } open:
                    result.Fallback = new ParentedExpression(null, open);
                    break;

                case KeywordToken:
                case BracketToken { Bracket: '}' }:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        if (!areTokensLeft || result.Fallback is null)
        {
            return result;
        }

        ParseList(ref result.Fallback);

        while (result.Comma is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BinaryOpToken { Type: OperatorType.Comma } comma:
                    result.Comma = comma;
                    break;

                case BracketToken { Bracket: '}' }:
                case KeywordToken:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        return result;
    }

    private static void ParseList(ref ParentedExpression result)
    {
        ExpressionAST? root = null;

        while (result.ClosingBracket is null && areTokensLeft)
        {
            var token = Tokens[Pointer];
            switch (token)
            {
                case KeywordToken:
                case BracketToken { Bracket: '}' }:
                    result.Expression = root;
                    return;

                case BracketToken { Bracket: ']' } listCloser:
                    result.ClosingBracket = listCloser;
                    break;

                case BinaryOpToken { Type: OperatorType.Comma } comma:
                    root = new BinaryOperation(null, root, comma);
                    break;

                case BaseValueToken identifier and not NumericToken:
                    switch (root)
                    {
                        case null:
                            root = identifier.ToAST(null);
                            break;

                        case BinaryOperation { Right: null } sequence:
                            sequence.Right = identifier.ToAST(sequence);
                            break;

                        default:
                            UnexpectedTokens.Add(token);
                            break;
                    }
                    break;

                default:
                    UnexpectedTokens.Add(token);
                    break;
            }
            Pointer++;
        }

        result.Expression = root;
    }
}