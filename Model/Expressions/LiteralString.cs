using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Expressions;

internal sealed class LiteralString(BaseExpression? parent, StringToken token)
    : BaseValueNode<StringToken>(parent, token)
{
    public override void VerifySyntax(DiagnosticContext context)
    {
        // TODO: validate start and end quote, supply DiagnosticContext with means to do so
    }
}