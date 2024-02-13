using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class Identifier : BaseValueNode<IdentifierToken>
{
    public SymbolKind kind
    {
        get => Token.kind;
        set => Token.kind = value;
    }

    public new IdentifierToken token => Token;

    public Identifier(ExpressionAST? parent, IdentifierToken token) : base(parent, token)
    { }
}