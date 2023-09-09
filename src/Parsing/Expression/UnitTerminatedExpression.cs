using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class UnitTerminatedExpression : ExpressionAST
{
    private ExpressionAST? _body;
    private UnitToken _token;

    public UnitTerminatedExpression(ExpressionAST? child, UnitToken token) : base(null)
    {
        _body = child;
        _token = token;
    }

    public override void Replace(ExpressionAST target, ExpressionAST value) => throw new Exception();
}