using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class UnitTerminatedExpression : ExpressionAST
{
    private readonly ExpressionAST? _child;
    private readonly UnitToken _token;

    public override int start => _child?.start ?? _token.Start;
    public override int end => _token.Start + _token.length;

    public UnitTerminatedExpression(ExpressionAST? child, UnitToken token) : base(null)
    {
        _child = child;
        _token = token;
    }
}