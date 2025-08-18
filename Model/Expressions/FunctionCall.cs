using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;

namespace NMLServer.Model.Expressions;

internal sealed class FunctionCall(BaseExpression? parent, BaseMulticharToken function) : BaseExpression(parent)
{
    public readonly BaseMulticharToken Function = function;

    /// <remarks>If not null, has not null opening paren.</remarks>
    public ParentedExpression? Arguments;

    public override int Start => Function.Start;
    public override int End => Arguments?.End ?? Function.End;

    // TODO: extend rules
    public override void VerifySyntax(DiagnosticContext context)
    {
        if (Function is not (IdentifierToken or KeywordToken { IsExpressionUsable: true }))
            context.Add(ErrorStrings.Err_InvalidFunctionId, Function);
        if (Arguments is null)
        {
            context.Add(ErrorStrings.Err_ExpectedFuncArguments, Function.End);
            return;
        }
        Arguments.VerifySyntax(context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "FunctionCall")
            .WithExprStyle();

        Function.Visualize(graph, n, ctx);
        Arguments.MaybeVisualize(graph, n, ctx);
        return n;
    }
}