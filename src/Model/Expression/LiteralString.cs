using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class LiteralString(ExpressionAST? parent, StringToken token)
    : BaseValueNode<StringToken>(parent, token)
{
    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        // TODO: validate start and end quote, supply DiagnosticContext with means to do so
    }
}