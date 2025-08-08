using NMLServer.Model.Diagnostics;
using NMLServer.Model.Expressions;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Statements.Blocks;

internal class FlowControlBlock(
    BaseParentStatement? parent,
    ref ParsingState state,
    KeywordToken keyword) : BaseParentStatement(parent, ref state, keyword)
{
    private readonly KeywordToken _keyword = keyword;

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (_keyword.Keyword is Keyword.Else)
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
                context.Add(BaseExpression.Errors.ErrorMissingExpr, _keyword.End);
        }
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel("If/Else");
}