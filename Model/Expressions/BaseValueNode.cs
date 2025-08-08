using NMLServer.Model.Tokens;
using DotNetGraph.Core;

namespace NMLServer.Model.Expressions;

internal abstract class BaseValueNode(BaseExpression? parent) : BaseExpression(parent)
{
    public abstract BaseValueToken Token { get; }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => Token.Visualize(graph, parent, ctx);
}

internal abstract class BaseValueNode<T>(BaseExpression? parent, T value) : BaseValueNode(parent)
    where T : BaseValueToken
{
    public sealed override T Token => value;

    public sealed override int Start => value.Start;
    public sealed override int End => value.End;
}