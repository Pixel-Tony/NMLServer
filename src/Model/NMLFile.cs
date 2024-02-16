using System.Runtime.CompilerServices;
using NMLServer.Lexing;
using NMLServer.Model.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer.Model;

/* One ring to rule them all */
internal readonly struct NMLFile
{
    private readonly List<BaseStatement>? _children = new();
    private readonly Dictionary<IdentifierToken, List<IdentifierToken>> _definedSymbols;
    private readonly List<Token> _tokens;
    private readonly List<Token> _unexpectedTokens = new();

    public IEnumerable<Token> tokens => _tokens;

    public bool isEmpty => _tokens.Count == 0;

    public IReadOnlyList<Token> unexpectedTokens => _unexpectedTokens;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NMLFile(List<Token> tokens, IIdentifierTokenComparer comparer)
    {
        _tokens = tokens;
        var state = new ParsingState(tokens, _unexpectedTokens);
        {
            BracketToken? ignored = null;
            _children = BaseStatement.ParseSomeInBlock(state, ref ignored, false);
        }
        _definedSymbols = new Dictionary<IdentifierToken, List<IdentifierToken>>(comparer);
        if (_children is null)
        {
            return;
        }
        foreach (var child in _children)
        {
            if (child is not ISymbolSource symbolSource)
            {
                continue;
            }
            var symbol = symbolSource.symbol;
            if (symbol is null)
            {
                continue;
            }
            if (_definedSymbols.TryGetValue(symbol, out var equalSymbols))
            {
                equalSymbols.Add(symbol);
                continue;
            }
            _definedSymbols[symbol] = new List<IdentifierToken> { symbol };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Diagnostic> ProvideDiagnostics(Document document)
    {
        if (_children is null)
        {
            return Array.Empty<Diagnostic>();
        }
        DiagnosticsContext ctx = new();
        foreach (var node in _children)
        {
            if (node is IValidatable validatable)
            {
                validatable.ProvideDiagnostics(ctx);
            }
        }
        return ctx.GetDiagnostics(document);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Token? GetTokenAt(int coordinate)
    {
        int max = _tokens.Count - 1;
        for (int left = 0, right = max; left <= right;)
        {
            int mid = left + (right - left) / 2;
            var current = _tokens[mid];
            int start = current.Start;
            if (coordinate < start)
            {
                right = mid - 1;
                continue;
            }
            var tokenLength = current.GetLength();
            if (start + tokenLength >= coordinate)
            {
                return current;
            }
            left = mid + 1;
        }
        return null;
    }

    public IReadOnlyList<IdentifierToken>? GetSourcesForSymbol(IdentifierToken symbol)
    {
        _definedSymbols.TryGetValue(symbol, out var list);
        return list;
    }
}