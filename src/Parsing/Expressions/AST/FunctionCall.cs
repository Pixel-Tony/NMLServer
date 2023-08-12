using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class FunctionCall : ExpressionAST
{
    public readonly BaseRecordingToken Function;
    public ParentedExpression? Arguments;

    public FunctionCall(ExpressionAST? parent, BaseRecordingToken function) : base(parent) => Function = function;

    public override string ToString() => $"(Func: {Function.value}{Arguments})";
}