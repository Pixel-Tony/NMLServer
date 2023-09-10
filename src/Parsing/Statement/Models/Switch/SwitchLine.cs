using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement.Models;

internal record struct SwitchLine
{
    public ExpressionAST? Condition;
    public RangeToken? Range;
    public ExpressionAST? ConditionRightSide;
    public ColonToken? Colon;
    public KeywordToken? ReturnKeyword;
    public ExpressionAST? Value;
    public SemicolonToken? Semicolon;
}