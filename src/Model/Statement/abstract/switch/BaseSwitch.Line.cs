using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class BaseSwitch
{
    private protected record struct Line(ExpressionAST? Condition, RangeToken? Range, ExpressionAST? ConditionRightSide,
        ColonToken? Colon, ReturnLine ReturnLine)
    {
        public ExpressionAST? Condition = Condition;
        public RangeToken? Range = Range;
        public ExpressionAST? ConditionRightSide = ConditionRightSide;
        public ColonToken? Colon = Colon;
        public ReturnLine ReturnLine = ReturnLine;
    }
}