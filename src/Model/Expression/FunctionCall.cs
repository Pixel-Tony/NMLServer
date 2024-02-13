using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class FunctionCall : ExpressionAST
{
    public readonly BaseMulticharToken Function;
    /* has opening paren if not null */
    public ParentedExpression? Arguments;

    public override int start => Function.Start;
    public override int end => Arguments?.end ?? start + Function.Length;

    public FunctionCall(ExpressionAST? parent, BaseMulticharToken function) : base(parent)
    {
        Function = function;
    }
}