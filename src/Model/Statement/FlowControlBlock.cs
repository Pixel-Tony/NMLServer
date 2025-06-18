using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class FlowControlBlock(
    InnerStatementNode? parent,
    ref ParsingState state,
    KeywordToken keyword) : InnerStatementNode(parent, ref state, keyword, new ParamInfo(0, 1, -1, true))
{
    private readonly KeywordType _keywordType = keyword.Type;

    // TODO Requires special parameter handling
    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (_keywordType != KeywordType.Else)
            base.VerifySyntax(in context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel("If/Else");
}