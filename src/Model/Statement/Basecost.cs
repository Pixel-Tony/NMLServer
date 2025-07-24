using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class Basecost(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<NMLAttribute>(ref state, keyword, ParamInfo.None)
{
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel("Basecost");
}