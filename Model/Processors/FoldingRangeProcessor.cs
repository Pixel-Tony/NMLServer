using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal sealed class FoldingRangeProcessor : IIncrementalNodeProcessor
{
    public readonly List<FoldingRange> Content = [];

    public void ProcessChangedSyntax(ref TreeTraverser traverser, BaseStatement? end, ref readonly IncrementContext context)
    {
        Content.Clear();
        ref readonly var tree = ref context.SyntaxTree;
        PositionConverter converter = tree.Tokens.MakeConverter();
        for (TreeTraverser trv = new(in tree); trv.Current is { } node; trv.Increment())
            node.ProvideFoldingRanges(Content, ref converter);
    }
}
