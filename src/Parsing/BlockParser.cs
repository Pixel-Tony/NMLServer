using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;
using NMLServer.Parsing.Statement;

namespace NMLServer.Parsing;

internal record struct StatementHeadingParseResult(KeywordToken Keyword)
{
    public readonly KeywordToken Keyword = Keyword;
    public ExpressionAST? Parameters;

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
        Pointer++;
        if (!areTokensLeft)
        {
            return;
        }

        var current = Tokens[Pointer];
        switch (current)
        {
            case UnitToken:
            case ColonToken:
            case FailedToken:
            case AssignmentToken:
                UnexpectedTokens.Add(current);
                goto start;

            case BracketToken { Bracket: '{' or '}' }:
            case KeywordToken { IsExpressionUsable: false }:
            case SemicolonToken:
                return;

            case BracketToken:
            case KeywordToken:
            case UnaryOpToken:
            case BinaryOpToken:
            case TernaryOpToken:
            case BaseValueToken:
                TryParseExpression(out result.Parameters, out _);
                return;

            default:
                throw new Exception();
        }
    }

    protected static FunctionStatement ParseFunctionStatement(KeywordToken keyword)
    {
        var result = new FunctionStatement();
        StatementHeading(keyword, out result.Heading);

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
                    Pointer++;
                    return result;

                case BracketToken:
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

    protected static BaseStatement ParseBlockStatement(KeywordToken keyword)
    {
        switch (keyword.Type)
        {
            case KeywordType.Grf:
                return GRFBlockParser.Apply(keyword);

            case KeywordType.BaseCost:
                return BasecostParser.Apply(keyword);

            case KeywordType.TramTypeTable:
            case KeywordType.RailTypeTable:
            case KeywordType.RoadTypeTable:
                return TracktypeTableParser.Apply(keyword);

            default:
                UnexpectedTokens.Add(keyword);
                Pointer = Max;
                return null!;
        }
    }
}