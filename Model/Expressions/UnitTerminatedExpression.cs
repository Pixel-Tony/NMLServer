using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Processors.Diagnostics;

namespace NMLServer.Model.Expressions;

internal sealed class UnitTerminatedExpression(BaseExpression? child, UnitToken token) : BaseExpression(null)
{
    public override int Start => child?.Start ?? token.Start;
    public override int End => token.End;

    public override void VerifySyntax(DiagnosticContext context)
    {
        if (child is null)
        {
            context.Add(ErrorStrings.ErrorMissingExpr, int.Max(Start - 1, 0));
            return;
        }
        child.VerifySyntax(context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "UnitExpr").WithExprStyle();
        child.MaybeVisualize(graph, n, ctx);
        return n;
    }
}