using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Expressions;

internal sealed class Number(BaseExpression? parent, NumericToken token) : BaseValueNode<NumericToken>(parent, token)
{
    public override void VerifySyntax(DiagnosticContext context)
    {
        // TODO
    }
}