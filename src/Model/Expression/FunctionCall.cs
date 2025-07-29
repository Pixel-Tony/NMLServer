using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Expression;

internal sealed class FunctionCall(ExpressionAST? parent, BaseMulticharToken function) : ExpressionAST(parent)
{
    public readonly BaseMulticharToken Function = function;

    /// <remarks>If not null, has not null opening paren.</remarks>
    public ParentedExpression? Arguments;

    public override int Start => Function.Start;
    public override int End => Arguments?.End ?? Function.End;

    // TODO: extend rules
    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (Function is not (IdentifierToken or KeywordToken { Kind: KeywordKind.ExpressionUsable }))
        {
            context.Add("Invalid function identifier", Function);
        }
        // TODO: verify function
        if (Arguments is null)
        {
            context.Add("Expected function arguments", Function.End);
            return;
        }
        Arguments.VerifySyntax(in context);
    }

#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "FunctionCall")
            .WithExprStyle();

        Function.Visualize(graph, n, ctx);
        Arguments.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}