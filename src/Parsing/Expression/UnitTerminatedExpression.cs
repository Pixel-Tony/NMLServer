using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class UnitTerminatedExpression : ExpressionAST
{
    private readonly ExpressionAST? _child;
    private readonly UnitToken _token;

    public UnitTerminatedExpression(ExpressionAST? child, UnitToken token) : base(null)
    {
        _child = child;
        _token = token;
    }
}