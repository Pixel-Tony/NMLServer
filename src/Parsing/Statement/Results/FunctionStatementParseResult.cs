using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement.Results;

internal record struct FunctionStatementParseResult(KeywordToken Keyword) : IRefineable
{
    public readonly KeywordToken Keyword = Keyword;
    public ExpressionAST? Parameters;
    public SemicolonToken? Semicolon;

    public BaseStatementAST Refine()
    {
        throw new NotImplementedException();
    }
}