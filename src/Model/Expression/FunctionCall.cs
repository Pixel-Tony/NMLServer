using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class FunctionCall : ExpressionAST
{
    public readonly BaseMulticharToken Function;
    public ParentedExpression? Arguments;

    public FunctionCall(ExpressionAST? parent, BaseMulticharToken function) : base(parent)
    {
        Function = function;
    }
}