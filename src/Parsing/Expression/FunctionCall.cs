using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class FunctionCall : ExpressionAST
{
    public readonly MulticharToken Function;
    public ParentedExpression? Arguments;

    public FunctionCall(ExpressionAST? parent, MulticharToken function) : base(parent)
    {
        Function = function;
    }
}