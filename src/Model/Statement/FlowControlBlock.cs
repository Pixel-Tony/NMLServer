#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using DotNetGraph.Extensions;
#endif
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Expression;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class FlowControlBlock(
    InnerStatementNode? parent,
    ref ParsingState state,
    KeywordToken keyword) : InnerStatementNode(parent, ref state, keyword, new ParamInfo(true, (0, 1)))
{
    private readonly KeywordToken _keyword = keyword;

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (_keyword.Type == KeywordType.Else)
        {
            if (Parameters is not null)
                context.Add(Errors.UnexpectedTopLevelExpr, _keyword.End);
        }
        else
        {
            base.VerifySyntax(in context);

            if (Parameters is not null)
                Parameters.VerifySyntax(in context);
            else
                context.Add(ExpressionAST.Errors.ErrorMissingExpr, _keyword.End);
        }
    }

#if TREE_VISUALIZER_ENABLED

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel("If/Else");
#endif
}