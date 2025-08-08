using NMLServer.Model.Diagnostics;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Expressions;

internal sealed class Number(BaseExpression? parent, NumericToken token) : BaseValueNode<NumericToken>(parent, token)
{
    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        // TODO
    }
}