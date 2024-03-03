using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class FunctionCall(ExpressionAST? parent, BaseMulticharToken function) : ExpressionAST(parent)
{
    public readonly BaseMulticharToken Function = function;
    /* has opening paren if not null */
    public ParentedExpression? Arguments;

    public override int start => Function.start;
    public override int end => Arguments?.end ?? start + Function.length;
}