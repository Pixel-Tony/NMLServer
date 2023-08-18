using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

using NamesAttributeValue = Pair<NumericToken, ExpressionAST>;
using NotImplementedException = Exception;

internal class GRFBlockParser : BaseParser
{
    /// <summary>Try to parse <c>grf {...}</c> block from input.</summary>
    /// <param name="parent">A parent to set for output block.</param>
    /// <param name="grf">A 'grf' keyword that should be already parsed.</param>
    /// <returns>Tuple containing GRFBlock instance and ending token. Token can be null if EOF is reached.</returns>
    public static (GRFBlock block, Token? ender) ParseGRFBlock(NMLFileRoot? parent, KeywordToken grf)
    {
        GRFBlock root = new(parent, grf);
        Pointer++;
        if (Pointer > Max)
        {
            goto lbl_ReturnOnOverflow;
        }
        lbl_CatchingOpeningBracket:
        var current = Tokens[Pointer];
        switch (current)
        {
            case FailedToken:
            case UnaryOpToken:
            case BinaryOpToken:
            case TernaryOpToken:
            case BaseValueToken:
                UnexpectedTokens.Add(current);
                Pointer++;
                if (Pointer >= Max)
                {
                    goto lbl_ReturnOnOverflow;
                }
                goto lbl_CatchingOpeningBracket;
            case KeywordToken:
                UnexpectedTokens.Add(current);
                goto lbl_ReturnOnOverflow;
            case BracketToken { Bracket: '{' } opening:
                root.OpeningBracket = opening;
                Pointer++;
                break;
            case ColonToken:
            case BracketToken:
            case SemicolonToken:
                UnexpectedTokens.Add(current);
                return (root, current);
        }
        // 5-6 attributes are at default in grf block
        List<NMLAttribute> attributes = new(6);
        List<GRFParameter> parameters = new();
        while (Pointer < Max)
        {
            current = Tokens[Pointer];
            switch (current)
            {
                case KeywordToken { value: "param" } paramKeyword:
                {
                    var (body, ender) = ParseParameter(root, paramKeyword);
                    parameters.Add(body);
                    switch (ender)
                    {
                        case null:
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                }
                case IdentifierToken:
                case ColonToken:
                {
                    var (attr, ender) = ParseAttribute(current);
                    attributes.Add(attr);
                    switch (ender)
                    {
                        case null:
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                }
                case BracketToken { Bracket: '}' } closingBracket:
                    root.ClosingBracket = closingBracket;
                    goto end;
                case BracketToken:
                case FailedToken:
                case BaseValueToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case UnaryOpToken:
                    UnexpectedTokens.Add(current);
                    break;
                case SemicolonToken semicolonToken:
                {
                    attributes.Add(new NMLAttribute(Semicolon: semicolonToken));
                    break;
                }
            }
            Pointer++;
        }
        end:
        root.Attributes = attributes.ToArray();
        root.Parameters = parameters.ToArray();
        lbl_ReturnOnOverflow:
        return (root, null);
    }

    private static (NMLAttribute, Token?) ParseAttribute(Token current)
    {
        NMLAttribute result;
        switch (current)
        {
            case IdentifierToken identifierToken:
                result = new NMLAttribute(identifierToken);
                break;
            default:
                throw new NotImplementedException();
        }
        Pointer++;
        if (Pointer > Max)
        {
            return (result, null);
        }
        switch (Tokens[Pointer])
        {
            case ColonToken colonToken:
                result.Colon = colonToken;
                break;
            default:
                throw new NotImplementedException();
        }
        Pointer++;
        if (Pointer > Max)
        {
            return (result, null);
        }
        var (expr, ender) = ExpressionParser.Apply();
        result.Value = expr;
        switch (ender)
        {
            case SemicolonToken semicolonToken:
                result.Semicolon = semicolonToken;
                return (result, null);
            default:
                return (result, ender);
        }
    }

    private static (GRFParameter, Token?) ParseParameter(GRFBlock root, KeywordToken alwaysParam)
    {
        GRFParameter result = new(root, alwaysParam);
        Pointer++;
        start:
        if (Pointer >= Max)
        {
            goto end;
        }
        switch (Tokens[Pointer])
        {
            case BracketToken { Bracket: '{' } opening:
                result.OpeningBracket = opening;
                break;
            case NumericToken number:
                if (result.ParameterNumber != null)
                {
                    UnexpectedTokens.Add(number);
                }
                else
                {
                    result.ParameterNumber = number;
                }
                Pointer++;
                goto start;
            default:
                throw new NotImplementedException();
        }
        Pointer++;
        if (Pointer >= Max)
        {
            goto end;
        }
        switch (Tokens[Pointer])
        {
            case IdentifierToken parameterName:
                result.Name = parameterName;
                break;
            default:
                throw new NotImplementedException();
        }
        Pointer++;
        if (Pointer >= Max)
        {
            goto end;
        }
        switch (Tokens[Pointer])
        {
            case BracketToken { Bracket: '{' } openingInner:
                result.InnerOpeningBracket = openingInner;
                break;
            default:
                throw new NotImplementedException();
        }
        Pointer++;
        List<NMLAttribute> attributes = new();
        List<NamesAttribute> namesAttributes = new();
        while (Pointer < Max)
        {
            IdentifierToken? attributeName = null;
            ColonToken? colon = null;
            switch (Tokens[Pointer])
            {
                case BracketToken { Bracket: '}' } bracketToken:
                    throw new NotImplementedException();
                case IdentifierToken identifierToken:
                    attributeName = identifierToken;
                    break;
                case ColonToken colonToken:
                    colon = colonToken;
                    goto afterColon;
                default:
                    throw new NotImplementedException();
            }
            Pointer++;
            if (Pointer >= Max)
            {
                attributes.Add(new NMLAttribute(attributeName));
                continue;
            }
            switch (Tokens[Pointer])
            {
                case ColonToken colonToken:
                    colon = colonToken;
                    break;
                default:
                    throw new NotImplementedException();
            }
            afterColon:
            Pointer++;
            if (Pointer >= Max)
            {
                attributes.Add(new NMLAttribute(attributeName, colon));
                continue;
            }
            switch (Tokens[Pointer])
            {
                case BracketToken { Bracket: '{' } bracketToken:
                    var (body, ender) = ParseNamesAttribute(bracketToken);
                    NamesAttribute namesAttribute = new(attributeName, colon, body);
                    switch (ender)
                    {
                        case null:
                            namesAttributes.Add(namesAttribute);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                
                
                default:
                    throw new NotImplementedException();
            }
            Pointer++;
        }

        result.Attributes = attributes.ToArray();
        result.Names = namesAttributes.ToArray();
        end:
        return (result, null);
    }

    private static (ParameterNames, Token?) ParseNamesAttribute(BracketToken openingBracket)
    {
        ParameterNames result = new(openingBracket);
        Pointer++;
        if (Pointer >= Max)
        {
            return (result, null);
        }
        List<NamesAttributeValue> values = new();
        while (Pointer < Max)
        {
            NamesAttributeValue current;
            switch (Tokens[Pointer])
            {
                case NumericToken number:
                    current = new NamesAttributeValue(number);
                    break;
                case BracketToken { Bracket: '}' } closingBracket:
                    result.ClosingBracket = closingBracket;
                    goto end;
                default:
                    throw new NotImplementedException();
            }
            Pointer++;
            if (Pointer >= Max)
            {
                values.Add(current);
                continue;
            }
            switch (Tokens[Pointer])
            {
                case ColonToken colonToken:
                    current.Colon = colonToken;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Pointer++;
            if (Pointer >= Max)
            {
                values.Add(current);
                continue;
            }
            var (expression, ender) = ExpressionParser.Apply();
            switch (ender)
            {
                case SemicolonToken semicolonToken:
                    current.Value = expression;
                    current.Semicolon = semicolonToken;
                    values.Add(current);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        end:
        if (values.Count != 0)
        {
            result.Items = values.ToArray();
        }
        return (result, null);
    }
}