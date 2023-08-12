using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal abstract class BaseValueToken : BaseRecordingToken
{
    protected BaseValueToken(char c) : base(c)
    { }

    protected BaseValueToken()
    { }

    public abstract ValueNode<BaseValueToken> ToAST(ExpressionAST? parent);
}