using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class UnitTerminatedExpression(ExpressionAST? child, UnitToken token) : ExpressionAST(null)
{
    public override int start => child?.start ?? token.start;
    public override int end => token.start + token.length;
}