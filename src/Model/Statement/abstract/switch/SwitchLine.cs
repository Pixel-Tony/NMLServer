using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal record struct SwitchLine(ExpressionAST? Condition, RangeToken? Range, ExpressionAST? ConditionRightSide,
    ColonToken? Colon, SwitchReturnLine ReturnLine)
{
    public ExpressionAST? Condition = Condition;
    public RangeToken? Range = Range;
    public ExpressionAST? ConditionRightSide = ConditionRightSide;
    public ColonToken? Colon = Colon;
    public SwitchReturnLine ReturnLine = ReturnLine;
}