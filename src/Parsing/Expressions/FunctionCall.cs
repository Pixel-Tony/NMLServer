using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class FunctionCall : ExpressionAST
{
    public LiteralToken Function;
    public ExpressionAST? Arguments;
    public (BracketToken?, BracketToken?) Parens;

    public FunctionCall(LiteralToken function, ExpressionAST? arguments, (BracketToken?, BracketToken?) parens)
    {
        Function = function;
        Arguments = arguments;
        Parens = parens;
    }

    public override string ToString() => $"Func: name={Function.value}, Args={Arguments}";
}