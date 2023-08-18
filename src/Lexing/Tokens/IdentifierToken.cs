using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal class IdentifierToken : BaseValueToken
{
    public IdentifierToken(char c) : base(c)
    { }

    public IdentifierToken()
    { }

    public override ValueNode<BaseValueToken> ToAST(ExpressionAST? parent) => new Identifier(parent, this);

    public override string ToString() => value;
}