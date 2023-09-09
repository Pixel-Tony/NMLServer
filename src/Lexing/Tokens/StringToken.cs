using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal class StringToken : BaseValueToken
{
    public override ValueNode<BaseValueToken> ToAST(ExpressionAST? parent)
    {
        return new LiteralString(parent, this);
    }

    public StringToken(int start, int end) : base(start, end)
    { }
}