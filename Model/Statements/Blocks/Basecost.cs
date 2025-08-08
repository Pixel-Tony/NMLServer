using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class Basecost(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<PropertySetting>(ref state, keyword)
{
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel("Basecost");
}