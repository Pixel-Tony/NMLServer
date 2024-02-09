using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class Number : BaseValueNode
{
    public Number(ExpressionAST? parent, NumericToken token) : base(parent, token)
    { }
}