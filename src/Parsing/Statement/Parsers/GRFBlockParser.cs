using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing.Statement;

using NamesPair = Pair<NumericToken, ExpressionAST>;

internal sealed class GRFBlockParser : AttributeParser
{
    /*
     * <kw:grf> "{" [<id> : <expr> | <param-block>]* "}"
     */
    public static GRFBlock Apply(KeywordToken alwaysGRF)
    {
        Pointer++;
        if (!areTokensLeft)
        {
            return new GRFBlock(alwaysGRF);
        }

        GRFBlock result = new GRFBlock(alwaysGRF);

        while (areTokensLeft && result.OpeningBracket is null)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    result.OpeningBracket = openingBracket;
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    Pointer++;
                    return new GRFBlock(alwaysGRF)
                    {
                        ClosingBracket = closingBracket
                    };

                case IdentifierToken:
                case KeywordToken { Type: KeywordType.Param }:
                    goto bodyStart;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }

        bodyStart:
        var attributes = new List<NMLAttribute>(6);
        var parameters = new List<GRFParameter>(2);
        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    result.ClosingBracket = closingBracket;
                    Pointer++;
                    goto end;

                case IdentifierToken identifier:
                    attributes.Add(ParseAttribute(identifier));
                    continue;

                case KeywordToken { Type: KeywordType.Param } paramKeyword:
                    ParseParameter(paramKeyword, out var parameter);
                    parameters.Add(parameter);
                    continue;

                case KeywordToken { IsBlock: true }:
                    goto end;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }

        end:
        result.Attributes = attributes.Count > 0
            ? attributes.ToArray()
            : null;
        result.Parameters = parameters.Count > 0
            ? parameters.ToArray()
            : null;
        return result;
    }

    private static void ParseParameter(KeywordToken paramKeyword, out GRFParameter result)
    {
        result = new GRFParameter(paramKeyword);
        Pointer++;
        while (result.OpeningBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    result.OpeningBracket = openingBracket;
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    result.ClosingBracket = closingBracket;
                    return;

                case KeywordToken { IsExpressionUsable: true }:
                case BaseValueToken:
                case BinaryOpToken:
                case BracketToken:
                case TernaryOpToken:
                case UnitToken:
                case UnaryOpToken:
                    if (result.ParameterNumber is not null)
                    {
                        goto default;
                    }
                    TryParseExpression(out var paramNumber, out _);
                    result.ParameterNumber = paramNumber;
                    continue;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }

        ParseParameterBody(ref result);

        if (areTokensLeft && Tokens[Pointer] is BracketToken { Bracket: '}' } outerClosingBracket)
        {
            result.ClosingBracket = outerClosingBracket;
            Pointer++;
            return;
        }

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    result.ClosingBracket = closingBracket;
                    Pointer++;
                    return;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
    }

    private static void ParseParameterBody(ref GRFParameter parameter)
    {
        if (!areTokensLeft)
        {
            return;
        }

        while (parameter.InnerOpeningBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    parameter.InnerOpeningBracket = openingBracket;
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    parameter.InnerClosingBracket = closingBracket;
                    Pointer++;
                    return;

                case IdentifierToken parameterName when parameter.Name is null:
                    parameter.Name = parameterName;
                    break;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        if (!areTokensLeft)
        {
            return;
        }

        ParseInnerParameterBody(ref parameter);

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            if (current is not BracketToken { Bracket: '}' } bracket)
            {
                UnexpectedTokens.Add(current);
                Pointer++;
                continue;
            }
            parameter.ClosingBracket = bracket;
            break;
        }
    }

    private static void ParseInnerParameterBody(ref GRFParameter parameter)
    {
        List<NMLAttribute> attributes = new(5);
        List<NamesAttribute> namesAttributes = new();

        // Start parsing attributes and names blocks...
        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            NMLAttribute pair;
            switch (current)
            {
                case IdentifierToken identifier:
                    pair = new NMLAttribute(identifier);
                    Pointer++;
                    break;

                case ColonToken colonToken:
                    pair = new NMLAttribute(null, colonToken);
                    Pointer++;
                    break;

                case SemicolonToken semicolon:
                    attributes.Add(new NMLAttribute(Semicolon: semicolon));
                    Pointer++;
                    continue;

                case BracketToken { Bracket: '}' } bracket:
                    parameter.InnerClosingBracket = bracket;
                    Pointer++;
                    goto end;

                case KeywordToken { IsExpressionUsable: false }:
                    goto end;

                default:
                    UnexpectedTokens.Add(current);
                    Pointer++;
                    continue;
            }

            // If colon was already found, this block is skipped
            while (pair.Colon is null && areTokensLeft)
            {
                switch (current = Tokens[Pointer])
                {
                    case ColonToken colonToken:
                        pair.Colon = colonToken;
                        break;

                    case KeywordToken:
                    case BracketToken:
                        return;

                    default:
                        UnexpectedTokens.Add(current);
                        break;
                }
                Pointer++;
            }

            // On colon found Pointer is incremented, no need to increment before this block
            bool added = false;
            while (!added && areTokensLeft)
            {
                current = Tokens[Pointer];
                switch (current)
                {
                    case BracketToken { Bracket: '}' } closingBracket:
                        attributes.Add(pair);
                        parameter.InnerClosingBracket = closingBracket;
                        Pointer++;
                        return;

                    case BracketToken { Bracket: '{' } openingBracket:
                        namesAttributes.Add(ParseNamesAttribute(pair, openingBracket));
                        added = true;
                        continue;

                    case SemicolonToken semicolon:
                        pair.Semicolon = semicolon;
                        attributes.Add(pair);
                        added = true;
                        break;

                    case KeywordToken { IsExpressionUsable: false }:
                        goto end;


                    case UnitToken:
                    case BracketToken:
                    case KeywordToken:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                    {
                        TryParseExpression(out pair.Value, out var finalizer);
                        if (finalizer is SemicolonToken semicolon)
                        {
                            pair.Semicolon = semicolon;
                            Pointer++;
                        }

                        attributes.Add(pair);
                        added = true;
                        continue;
                    }

                    default:
                        UnexpectedTokens.Add(current);
                        break;
                }
                Pointer++;
            }

            if (!areTokensLeft)
            {
                attributes.Add(pair);
            }
        }

        end:
        parameter.Attributes = attributes.Count > 0
            ? attributes.ToArray()
            : null;

        parameter.Names = namesAttributes.Count > 0
            ? namesAttributes.ToArray()
            : null;
    }

    private static NamesAttribute ParseNamesAttribute(NMLAttribute sketch, BracketToken openingBracket)
    {
        var result = NamesAttribute.From(sketch, new ParameterNames(openingBracket));
        List<NamesPair> content = new();
        Pointer++;

        // Parsing body
        while (result.Value.ClosingBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case NumericToken number:
                    content.Add(ParseAttribute(number));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    result.Value.ClosingBracket = closingBracket;
                    Pointer++;
                    break;

                case KeywordToken:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    Pointer++;
                    break;
            }
        }
        result.Value.Items = content.Count > 0
            ? content.ToArray()
            : null;

        // Parsing closing bracket
        while (result.Value.ClosingBracket is null && areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closing:
                    result.Value.ClosingBracket = closing;
                    break;

                case SemicolonToken semicolonToken:
                    result.Semicolon = semicolonToken;
                    return result;

                case KeywordToken:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        if (!areTokensLeft || Tokens[Pointer] is not SemicolonToken semicolon)
        {
            return result;
        }
        result.Semicolon = semicolon;
        Pointer++;
        return result;
    }
}