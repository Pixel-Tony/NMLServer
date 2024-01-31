using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal sealed class Number : BaseValueNode
{
    public Number(ExpressionAST? parent, NumericToken token) : base(parent, token)
    { }
}