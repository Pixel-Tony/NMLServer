using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

using NamesAttributePair = Pair<NumericToken, ExpressionAST>;

internal sealed class GRFBlockParser : AttributeParser
{
    /*
     * <kw:grf> "{" [<id> : <expr> | <param-block>]* "}"
     */
    public static GRFBlock Apply(KeywordToken alwaysGRF, BaseStatementAST parent)
    {
        Pointer++;
        if (Pointer >= Max)
        {
            return new GRFBlock(parent, alwaysGRF);
        }

        BracketToken? openingBracket = null;
        BracketToken? closingBracket = null;

        while (Pointer < Max && openingBracket is null)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' } expectedOpeningBracket:
                    openingBracket = expectedOpeningBracket;
                    break;

                case BracketToken { Bracket: '}' } unexpectedClosingBracket:
                    Pointer++;
                    return new GRFBlock(parent, alwaysGRF)
                    {
                        _closingBracket = unexpectedClosingBracket
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
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case IdentifierToken identifier:
                    attributes.Add(ParseAttribute(identifier));
                    continue;

                case KeywordToken { Type: KeywordType.Param } paramKeyword:
                    TryParseParameter(paramKeyword, out var parameter);
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
        return new GRFBlock(parent, alwaysGRF)
        {
            _openingBracket = openingBracket,
            _attributes = attributes.ToArray(),
            _parameters = parameters.ToArray(),
            _closingBracket = closingBracket
        };
    }

    private static void TryParseParameter(KeywordToken paramKeyword, out GRFParameter result)
    {
        result = new GRFParameter(paramKeyword);
        Pointer++;
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    result.OpeningBracket = openingBracket;
                    Pointer++;
                    goto openingBracketParsed;

                case BracketToken { Bracket: '}' } closingBracket:
                    result.ClosingBracket = closingBracket;
                    return;

                case KeywordToken { IsExpressionUsable: false }:
                case BaseValueToken:
                case BinaryOpToken:
                case BracketToken:
                case TernaryOpToken:
                case UnitToken:
                case UnaryOpToken:
                    if (result.ParameterNumber != null)
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

    openingBracketParsed:
        ParseParameterBody(ref result);

        if (Pointer < Max && Tokens[Pointer] is BracketToken { Bracket: '}' } outerClosingBracket)
        {
            result.ClosingBracket = outerClosingBracket;
            Pointer++;
            return;
        }

        while (Pointer < Max)
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
        if (Pointer >= Max)
        {
            return;
        }

        while (Pointer < Max && parameter.InnerOpeningBracket is null)
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
        if (Pointer >= Max)
        {
            return;
        }

        ParseInnerParameterBody(ref parameter);

        while (Pointer < Max)
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
        List<NamesAttribute> namesAttributes = null!;

        // Start parsing attributes and names blocks...
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            NMLAttribute pair = new();
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
                    goto end;

                case KeywordToken { IsExpressionUsable: false }:
                    goto end;

                default:
                    UnexpectedTokens.Add(current);
                    Pointer++;
                    continue;
            }

            // If colon was already found, this block is skipped
            while (pair.Colon is null && Pointer < Max)
            {
                switch (current = Tokens[Pointer])
                {
                    case ColonToken colonToken:
                        pair.Colon = colonToken;
                        Pointer++;
                        break;

                    default:
                        UnexpectedTokens.Add(current);
                        break;
                }
                Pointer++;
            }

            // On colon found Pointer is incremented, no need to increment before this block
            bool added = false;
            while (!added && Pointer < Max)
            {
                switch (current = Tokens[Pointer])
                {
                    case BracketToken { Bracket: '}' } closingBracket:
                        attributes.Add(pair);
                        parameter.InnerClosingBracket = closingBracket;
                        Pointer++;
                        return;

                    case BracketToken { Bracket: '{' } openingBracket:
                        namesAttributes ??= new List<NamesAttribute>();
                        namesAttributes.Add(ParseNamesAttribute(pair, openingBracket));
                        added = true;
                        break;

                    case SemicolonToken semicolon:
                        pair.Semicolon = semicolon;
                        attributes.Add(pair);
                        added = true;
                        break;

                    case KeywordToken { IsExpressionUsable: false}:
                        throw new NotImplementedException();

                    case AssignmentToken:
                    case ColonToken:
                    case FailedToken:
                        UnexpectedTokens.Add(current);
                        break;

                    default:
                        ParseAttribute(ref pair);
                        attributes.Add(pair);
                        break;
                }
                Pointer++;
            }

            if (Pointer >= Max)
            {
                attributes.Add(pair);
            }
        }

        end:
        if (attributes.Count > 0)
        {
            parameter.Attributes = attributes.ToArray();
        }
        parameter.Names = namesAttributes?.ToArray();
    }

    private static NamesAttribute ParseNamesAttribute(NMLAttribute sketch, BracketToken openingBracket)
    {
        var result = NamesAttribute.From(sketch, new ParameterNames(openingBracket));
        List<NamesAttributePair> content = new();

        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case NumericToken number:
                    content.Add(ParseAttribute(number));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    result.Value.ClosingBracket = closingBracket;
                    return result;
            }
            Pointer++;
        }
        result.Value.Items = content.ToArray();
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            if (current is BracketToken { Bracket: '}' } closing)
            {
                result.Value.ClosingBracket = closing;
                break;
            }
            Pointer++;
        }

        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
                    Pointer++;
                    return result;

                case BracketToken { Bracket: '}' }:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        return result;
    }
}