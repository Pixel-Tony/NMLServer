using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class FunctionCall(ExpressionAST? parent, BaseMulticharToken function) : ExpressionAST(parent)
{
    public readonly BaseMulticharToken Function = function;

    /// <remarks>If not null, has not null opening paren.</remarks>
    public ParentedExpression? Arguments;

    public override int start => Function.start;
    public override int end => Arguments?.end ?? start + Function.length;
}