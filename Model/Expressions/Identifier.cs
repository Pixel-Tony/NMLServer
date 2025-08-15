using NMLServer.Model.Grammar;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Expressions;

internal sealed class Identifier(BaseExpression? parent, IdentifierToken token)
    : BaseValueNode<IdentifierToken>(parent, token)
{
    public SymbolKind Kind => Token.Kind;
}