using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class Identifier(ExpressionAST? parent, IdentifierToken token)
    : BaseValueNode<IdentifierToken>(parent, token)
{
    public SymbolKind kind
    {
        get => Token.kind;
        set => Token.kind = value;
    }

    public new IdentifierToken token => Token;
}