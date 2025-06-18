using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal abstract class InnerStatementNode : BlockStatement
{
    // TODO remove if not used, previously used only while constructing ast, can be replaced with a stack
    public readonly InnerStatementNode? Parent;
    public readonly List<StatementAST>? Children;

    protected sealed override int middleEnd => Children?[^1].end ?? 0;

    protected InnerStatementNode(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword,
        ParamInfo info) : base(ref state, keyword, info)
    {
        Parent = parent;
        if (OpeningBracket is not null && ClosingBracket is null)
        {
            Children = [];
        }
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx)
            .WithLabel("Node");

        foreach (var child in Children ?? [])
            child.Visualize(graph, n, ctx);

        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}