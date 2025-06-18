using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed class SpriteGroup(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<NMLAttribute>(ref state, keyword, new ParamInfo(1, 1, 0, false))
{
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => base.Visualize(graph, parent, ctx)
            .WithLabel("Spritegroup");
}