using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal abstract class ValueNode<T> : ExpressionAST, IHoldsSingleToken where T : BaseRecordingToken
{
    public BaseRecordingToken token { get; }

    protected ValueNode(ExpressionAST? parent, T recordingToken) : base(parent)
    {
        token = recordingToken;
    }

    public sealed override ExpressionAST Replace(ExpressionAST target, ExpressionAST value)
    {
        return base.Replace(target, value);
    }

    public override string ToString() => token.value;
}