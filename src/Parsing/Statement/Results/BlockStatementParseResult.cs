using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement.Results;

internal record struct BlockStatementParseResult(BaseRecordingToken Keyword) : IRefineable
{
    public readonly BaseRecordingToken Keyword = Keyword;
    public ExpressionAST? Parameters;
    public StatementBody Body = new();
    
    public BaseStatementAST Refine()
    {
        throw new NotImplementedException();
    }
}