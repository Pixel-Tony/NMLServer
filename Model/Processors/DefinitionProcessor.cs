using NMLServer.Extensions;
using NMLServer.Model.Statements;
using NMLServer.Model.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace NMLServer.Model.Processors;

internal sealed class DefinitionProcessor() : IIncrementalNodeProcessor
{
    public IReadOnlyDictionary<string, List<IdentifierToken>> Symbols => _symbols;
    private readonly Dictionary<string, List<IdentifierToken>> _symbols = [];

    public bool Has(IdentifierToken symbol, StringView source, [NotNullWhen(true)] out List<IdentifierToken>? definitions)
    {
        var context = symbol.Context(source);
        var lookup = _symbols.GetAlternateLookup<StringView>();
        return lookup.TryGetValue(context, out definitions);
    }

    public void Add(IdentifierToken symbol, StringView source)
    {
        var context = symbol.Context(source);
        var lookup = _symbols.GetAlternateLookup<StringView>();
        if (!lookup.TryGetValue(context, out var definitions))
        {
            definitions = [];
            _symbols[new string(context)] = definitions;
        }
        definitions.Add(symbol);
    }

    public void Trim((int begin, int end) range, Range _)
    {
        var (start, end) = range;
        List<string> keysToRemove = [];
        foreach (var (key, values) in _symbols)
        {
            var first = int.Max(values.FindLastBefore(start), 0);
            var afterLast = values.FindFirstAfter(end, first + 1);
            values.RemoveRange(first, afterLast - first);
            if (values.Count == 0)
                keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove)
            _symbols.Remove(key);
    }

    public void Process(BaseStatement node, NodeProcessingContext context) => node.AddDefinitions(this, context.Source);
}
