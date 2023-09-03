using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal abstract class BaseValueToken : BaseRecordingToken
{
    protected BaseValueToken(int start, int end) : base(start, end)
    { }

    public abstract ValueNode<BaseValueToken> ToAST(ExpressionAST? parent);
}