using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statements.Models;

internal class Assignment : Statement
{
    public ExpressionAST? LeftHandSide;
    public AssignmentToken? EqualsSign;
    public ExpressionAST? RighHandSide;
    public SemicolonToken? Semicolon;

    public Assignment(ExpressionAST? leftHandSide)
    {
        LeftHandSide = leftHandSide;
    }

    public Assignment(ExpressionAST? leftHandSide, AssignmentToken? equalsSign)
    {
        LeftHandSide = leftHandSide;
        EqualsSign = equalsSign;
    }
}