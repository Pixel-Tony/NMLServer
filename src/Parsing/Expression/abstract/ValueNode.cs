using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal abstract class ValueNode<T> : ExpressionAST, IHoldsSingleToken where T : BaseRecordingToken
{
    private readonly T _token;
    public BaseRecordingToken token => _token;

    protected ValueNode(ExpressionAST? parent, T recordingToken) : base(parent)
    {
        _token = recordingToken;
    }

    public sealed override void Replace(ExpressionAST target, ExpressionAST value)
    {
        FailReplacement();
    }
}