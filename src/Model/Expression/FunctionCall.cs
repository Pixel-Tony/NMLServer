using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class FunctionCall(ExpressionAST? parent, BaseMulticharToken function) : ExpressionAST(parent)
{
    public readonly BaseMulticharToken Function = function;

    /// <remarks>If not null, has not null opening paren.</remarks>
    public ParentedExpression? Arguments;

    public override int start => Function.start;
    public override int end => Arguments?.end ?? Function.end;

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
            context.Add("Expected function arguments", Function.end);
            return;
        }
        Arguments.VerifySyntax(in context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "FunctionCall")
            .WithExprStyle();

        Function.Visualize(graph, n, ctx);
        Arguments.MaybeVisualize(graph, n, ctx);
        return n;
    }
}