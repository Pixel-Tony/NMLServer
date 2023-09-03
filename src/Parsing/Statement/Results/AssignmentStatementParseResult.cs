using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement.Results;

internal record struct AssignmentStatementParseResult : IRefineable
{
    public ExpressionAST? Lhs;
    public AssignmentToken AssignmentToken;
    public ExpressionAST? Rhs;
    public SemicolonToken SemicolonToken;
    
    public BaseStatementAST Refine()
    {
        throw new NotImplementedException();
    }
}