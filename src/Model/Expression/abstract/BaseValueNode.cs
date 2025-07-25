#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
#endif
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal abstract class BaseValueNode(ExpressionAST? parent) : ExpressionAST(parent)
{
    public abstract BaseValueToken Token { get; }

#if TREE_VISUALIZER_ENABLED

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => Token.Visualize(graph, parent, ctx);
#endif
}

internal abstract class BaseValueNode<T>(ExpressionAST? parent, T value) : BaseValueNode(parent)
    where T : BaseValueToken
{
    public sealed override T Token => value;

    public sealed override int Start => value.Start;
    public sealed override int End => value.End;
}