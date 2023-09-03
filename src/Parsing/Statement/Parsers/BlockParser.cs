using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;
using NMLServer.Parsing.Statement;
using NMLServer.Parsing.Statement.Results;

namespace NMLServer.Parsing;

using NamesPair = Pair<NumericToken, ExpressionAST>;

internal class BlockParser : BaseParser
{
    public static void FunctionLike(KeywordToken keyword, out FunctionStatementParseResult result)
    {
        result = new FunctionStatementParseResult(keyword);
        start:
        if (++Pointer >= Max)
        {
            return;
        }

        var current = Tokens[Pointer];
        switch (current)
        {
            case KeywordToken:
                throw new NotImplementedException();

            case ColonToken:
            case FailedToken:
                UnexpectedTokens.Add(current);
                goto start;
                
            case BracketToken { Bracket: '{' or '}' }:
                UnexpectedTokens.Add(current);
                return;

            case BracketToken:
            case UnaryOpToken:
            case TernaryOpToken:
            case BinaryOpToken:
            case BaseValueToken:
                var (expr, ender) = ExpressionParser.Apply();
                result.Parameters = expr;
                if (ender is SemicolonToken semicolonToken)
                {
                    result.Semicolon = semicolonToken;
                    return;
                }
                goto semicolonCatcher;

            case SemicolonToken semicolon:
                result.Semicolon = semicolon;
                return;
        }

        if (++Pointer >= Max)
        {
            return;
        }
        semicolonCatcher:
        //TODO:
        throw new Exception();
    }

    public static void BlockLike(KeywordToken keyword, out BlockStatementParseResult result)
    {
        result = new BlockStatementParseResult(keyword);
        start:
        if (++Pointer >= Max)
        {
            return;
        }
        firstSwitch:
        var current = Tokens[Pointer];
        switch (current)
        {
            case BracketToken { Bracket: '{' } openingBracket:
                result.Body.OpeningBracket = openingBracket;
                break;

            case BracketToken { Bracket: '}' } closingBracket:
                result.Body.ClosingBracket = closingBracket;
                Pointer++;
                return;

            case BracketToken { Bracket: ')' or ']' } closingBracket:
                result.Parameters = new ParentedExpression(null, null)
                {
                    Expression = result.Parameters,
                    ClosingBracket = closingBracket
                };
                goto start;

            case BracketToken:
            case UnaryOpToken:
            case BinaryOpToken:
            case TernaryOpToken:
            case BaseValueToken:
            case KeywordToken { IsExpressionUsable: true }:
                if (result.Parameters != null)
                {
                    UnexpectedTokens.Add(current);
                    goto start;
                }
                var (expr, _) = ExpressionParser.Apply();
                result.Parameters = expr;
                goto firstSwitch;

            case ColonToken:
            case FailedToken:
            case SemicolonToken:
                UnexpectedTokens.Add(current);
                goto start;

            case KeywordToken:
                return;
        }
        Pointer++;
        ParseBlockBody(ref result.Body);
    }

    private static void ParseBlockBody(ref StatementBody body)
    {
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    body.ClosingBracket = closingBracket;
                    Pointer++;
                    return;

                case KeywordToken { IsBlock: true } keywordToken:
                    BlockLike(keywordToken, out var subBlock);
                    body.Blocks.Add(subBlock);
                    continue;

                case IdentifierToken identifier:
                    NMLAttribute attribute = new(identifier);
                    if (++Pointer >= Max)
                    {
                        body.Attributes.Add(attribute);
                        return;
                    }
                    switch (Tokens[Pointer])
                    {
                        case ColonToken colonToken:
                        {
                            attribute.Colon = colonToken;
                            if (++Pointer >= Max)
                            {
                                body.Attributes.Add(attribute);
                                return;
                            }
                            if (Tokens[Pointer] is BracketToken { Bracket: '{' } openingBracket)
                            {
                                ParseNamesAttribute(identifier, colonToken, openingBracket, out var names);
                                body.NamesBlocks.Add(names);
                                continue;
                            }
                            var (expr, ender) = ExpressionParser.Apply();
                            attribute.Value = expr;
                            if (ender is SemicolonToken semicolonToken)
                            {
                                attribute.Semicolon = semicolonToken;
                                body.Attributes.Add(attribute);
                                break;
                            }
                            body.Attributes.Add(attribute);
                            continue;
                        }

                        case BracketToken { Bracket: '{' } openingBracket:
                            // Expected result - parameter body
                            var identifierBlock = new BlockStatementParseResult(identifier)
                            {
                                Body = new StatementBody(7, 0, 1)
                                {
                                    OpeningBracket = openingBracket
                                }
                            };
                            if (++Pointer < Max)
                            {
                                ParseBlockBody(ref identifierBlock.Body);
                            }
                            body.Blocks.Add(identifierBlock);
                            continue;

                        case BracketToken { Bracket: '}' } closingBracket:
                            body.ClosingBracket = closingBracket;
                            Pointer++;
                            return;

                        default:
                            //TODO
                            throw new Exception();
                    }
                    break;

                case FailedToken:
                    UnexpectedTokens.Add(current);
                    break;

                default:
                    //TODO
                    throw new Exception();
            }
            Pointer++;
        }
    }

    private static void ParseNamesAttribute(IdentifierToken names, ColonToken colonToken, BracketToken openingBracket,
        out NamesAttribute result)
    {
        result = new NamesAttribute(names, colonToken, new ParameterNames(openingBracket));
        List<NamesPair> namesList = new();
        ++Pointer;
        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case SemicolonToken semicolon when result.Value.ClosingBracket != null:
                    result.Semicolon = semicolon;
                    Pointer++;
                    return;

                case BracketToken { Bracket: '}' } closingBracket when result.Semicolon == null:
                    result.Value.Items = namesList.Count > 0
                        ? namesList.ToArray()
                        : null;
                    result.Value.ClosingBracket = closingBracket;
                    break;

                case NumericToken number:
                    var pair = AttributeParser.Apply(number);
                    namesList.Add(pair);
                    if (pair.Semicolon == null)
                    {
                        continue;
                    }
                    break;

                default:
                    // TODO
                    throw new Exception();
            }
            Pointer++;
        }
        result.Value.Items = namesList.Count > 0
            ? namesList.ToArray()
            : null;
    }
}