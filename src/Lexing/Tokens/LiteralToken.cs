using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal class LiteralToken : BaseValueToken
{
    public LiteralToken(char c) : base(c)
    { }

    public LiteralToken()
    { }

    public override ValueNode<BaseValueToken> ToAST(ExpressionAST? parent) => new Identifier(parent, this);
}