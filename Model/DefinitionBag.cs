using NMLServer.Model.Grammar;
using NMLServer.Model.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace NMLServer.Model;

internal sealed class DefinitionBag : Dictionary<string, List<(IdentifierToken identifier, SymbolKind kind)>>
{
    public bool Has(IdentifierToken symbol, StringView source,
        [NotNullWhen(true)] out List<(IdentifierToken identifierToken, SymbolKind kind)>? definitions)
    {
        var context = symbol.Context(source);
        var lookup = GetAlternateLookup<StringView>();
        return lookup.TryGetValue(context, out definitions);
    }

    public void Add(IdentifierToken symbol, SymbolKind kind, StringView source)
    {
        var context = symbol.Context(source);
        var lookup = GetAlternateLookup<StringView>();
        if (!lookup.TryGetValue(context, out var definitions))
        {
            definitions = [];
            this[new string(context)] = definitions;
        }
        definitions.Add((symbol, kind));
    }
}
