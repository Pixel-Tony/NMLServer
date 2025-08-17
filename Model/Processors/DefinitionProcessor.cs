using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal sealed class DefinitionProcessor : IIncrementalNodeProcessor
{
    public readonly DefinitionBag Symbols = [];

    public void ProcessChangedSyntax(ref TreeTraverser traverser, BaseStatement? end, ref readonly IncrementContext context)
    {
        Symbols.Clear();
        ref readonly var ast = ref context.SyntaxTree;
        var source = ast.Tokens.Source;
        for (TreeTraverser trv = new(in ast); trv.Current is { } node; trv.Increment())
            node.AddDefinitions(Symbols, source);
    }
}
