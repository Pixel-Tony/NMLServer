using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal class NumericToken : BaseValueToken
{
    public NumericToken(int start, int end) : base(start, end)
    { }

    public override ValueNode<BaseValueToken> ToAST(ExpressionAST? parent) => new Number(parent, this);
}