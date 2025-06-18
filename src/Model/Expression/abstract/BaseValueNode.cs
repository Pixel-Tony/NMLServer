using DotNetGraph.Core;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal abstract class BaseValueNode(ExpressionAST? parent) : ExpressionAST(parent)
{
    public abstract BaseValueToken token { get; }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => token.Visualize(graph, parent, ctx);
}

internal abstract class BaseValueNode<T>(ExpressionAST? parent, T value) : BaseValueNode(parent)
    where T : BaseValueToken
{
    public sealed override T token => value;

    public sealed override int start => value.start;
    public sealed override int end => value.end;
}