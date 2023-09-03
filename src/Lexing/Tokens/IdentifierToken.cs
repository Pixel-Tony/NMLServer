using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal class IdentifierToken : BaseValueToken
{
    public override ValueNode<BaseValueToken> ToAST(ExpressionAST? parent) => new Identifier(parent, this);

    public IdentifierToken(int start, int end) : base(start, end)
    { }
}