using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statements.Models;

internal record struct StatementHeading(KeywordToken Keyword)
{
    public readonly KeywordToken Keyword = Keyword;
    public ExpressionAST? Parameters;

    public StatementHeading() : this(null!)
    {
        throw new Exception($"Do not use parameterless constructor of type {nameof(StatementHeading)}");
    }
}