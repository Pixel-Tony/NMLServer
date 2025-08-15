using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal sealed class DefinitionProcessor : IIncrementalNodeProcessor
{
    public readonly DefinitionBag Symbols = [];

    public void Trim()
    {
        // var (start, end) = range;
        // List<string> keysToRemove = [];
        // foreach (var (key, values) in _symbols)
        // {
        //     var first = int.Max(values.FindLastBefore(start), 0);
        //     var afterLast = values.FindFirstAfter(end, first + 1);
        //     values.RemoveRange(first, afterLast - first);
        //     if (values.Count == 0)
        //         keysToRemove.Add(key);
        // }
        // foreach (var key in keysToRemove)
        //     _symbols.Remove(key);
    }

    public void Process(BaseStatement node, NodeProcessingContext context)
    {
        // node.AddDefinitions(this, context.Source);
    }

    public void FinishIncrement(ref readonly AbstractSyntaxTree ast)
    {
        Symbols.Clear();
        var source = ast.Tokens.Source;
        for (TreeTraverser trv = new(ast); trv.Current is { } node; trv.Increment())
            node.AddDefinitions(Symbols, source);
    }

}
