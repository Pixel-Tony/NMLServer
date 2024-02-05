using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class FunctionCall : ExpressionAST
{
    public readonly BaseMulticharToken Function;
    public ParentedExpression? Arguments;

    public FunctionCall(ExpressionAST? parent, BaseMulticharToken function) : base(parent)
    {
        Function = function;
    }
}