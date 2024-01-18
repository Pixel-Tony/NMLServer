using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class UnitTerminatedExpression : ExpressionAST
{
    private ExpressionAST? _child;
    private UnitToken _token;

    public UnitTerminatedExpression(ExpressionAST? child, UnitToken token) : base(null)
    {
        _child = child;
        _token = token;
    }

    protected override void Replace(ExpressionAST target, ExpressionAST value) => throw new Exception();
}