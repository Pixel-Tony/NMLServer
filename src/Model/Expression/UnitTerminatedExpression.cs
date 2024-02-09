using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class UnitTerminatedExpression : ExpressionAST
{
    private readonly ExpressionAST? _child;
    private readonly UnitToken _token;

    public UnitTerminatedExpression(ExpressionAST? child, UnitToken token) : base(null)
    {
        _child = child;
        _token = token;
    }
}