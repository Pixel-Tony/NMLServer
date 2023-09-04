using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class Assignment : BaseStatementAST
{
    public ExpressionAST? LeftHandSide;
    public AssignmentToken? EqualsSign;
    public ExpressionAST? RighHandSide;
    public SemicolonToken? Semicolon;

    public Assignment(BaseStatementAST? parent) : base(parent)
    { }
}