using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal class NumericToken : BaseValueToken
{
    public override ValueNode<BaseValueToken> ToAST(ExpressionAST? parent) => new Number(parent, this);
}