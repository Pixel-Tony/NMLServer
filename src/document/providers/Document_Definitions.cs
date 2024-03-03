using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal partial class Document
{
    // TODO: full -> incremental
    public LocationOrLocationLinks? ProvideDefinitions(Position position)
    {
        if (_statements.Count is 0 || _definedSymbols.Count is 0)
        {
            return null;
        }
        if (GetTokenAt(ProtocolToLocal(position)) is not IdentifierToken { length: var tokenLength } symbol
            || !_definedSymbols.TryGetValue(symbol, out var symbolDefinitions))
        {
            return null;
        }
        List<LocationOrLocationLink> locations = new(capacity: symbolDefinitions.Count);
        foreach (var sameSymbol in symbolDefinitions)
        {
            var (line, @char) = LocalToProtocol(sameSymbol.start);
            locations.Add(new Location
            {
                Uri = Uri,
                Range = new Range(line, @char, line, @char + tokenLength)
            });
        }
        return locations;
    }

    private Token? GetTokenAt(int coordinate)
    {
        for (int left = 0, right = _tokens.Count - 1; left <= right;)
        {
            int mid = left + (right - left) / 2;
            var current = _tokens[mid];
            if (coordinate < current.start)
            {
                right = mid - 1;
                continue;
            }
            if (coordinate <= current.end)
            {
                return current;
            }
            left = mid + 1;
        }
        return null;
    }
}