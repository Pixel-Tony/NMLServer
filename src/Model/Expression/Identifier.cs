using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class Identifier(ExpressionAST? parent, IdentifierToken token)
    : BaseValueNode<IdentifierToken>(parent, token)
{
    public SymbolKind kind => token.Kind;

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        // TODO
    }
}