using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;
using NMLServer.Parsing.Statement;

namespace NMLServer.Parsing;

using NamesPair = Pair<NumericToken, ExpressionAST>;

internal record struct StatementHeadingParseResult(KeywordToken Keyword)
{
    public readonly KeywordToken Keyword = Keyword;
    public ExpressionAST? Parameters;

    // ReSharper disable once UnusedMember.Global
    public StatementHeadingParseResult() : this(null!)
    {
        throw new Exception($"Do not use parameterless constructor of type {nameof(StatementHeadingParseResult)}");
    }
}

internal class BlockParser : AttributeParser
{
    private static void StatementHeading(KeywordToken keyword, out StatementHeadingParseResult result)
    {
        result = new StatementHeadingParseResult(keyword);
        start:
        if (++Pointer >= Max)
        {
            return;
        }

        var current = Tokens[Pointer];
        switch (current)
        {
            case ColonToken:
            case FailedToken:
                UnexpectedTokens.Add(current);
                goto start;

            case BracketToken { Bracket: '{' or '}' }:
            case KeywordToken { IsExpressionUsable: false }:
            case SemicolonToken:
                return;

            case UnaryOpToken:
            case BinaryOpToken:
            case TernaryOpToken:
            case BaseValueToken:
            case BracketToken:
            case KeywordToken:
                TryParseExpression(out result.Parameters, out _);
                return;

            default:
                throw new Exception();
        }
    }

    protected static FunctionStatement ParseFunctionStatement(NMLFileRoot? parent, KeywordToken keyword)
    {
        var result = new FunctionStatement(parent);
        StatementHeading(keyword, out result.Heading);

        while (Pointer < Max)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
                    Pointer++;
                    return result;

                case BracketToken:
                    UnexpectedTokens.Add(current);
                    return result;

                case KeywordToken { IsExpressionUsable: false }:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        return result;
    }

    protected static BaseStatementAST ParseBlockStatement(BaseStatementAST parent, KeywordToken keyword)
    {
        switch (keyword.Type)
        {
            case KeywordType.Grf:
                return GRFBlockParser.Apply(keyword, parent);
        }

        StatementHeading(keyword, out var heading);
        if (Pointer >= Max)
        {
            return new FunctionStatement(parent, heading);
        }
        throw new NotImplementedException();
    }
}