using NMLServer.Lexing;
using NMLServer.Model;
using NMLServer.Model.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

using DefinitionsMap = Dictionary<IdentifierToken, List<IdentifierToken>>;

internal partial class Document
{
    private string _source;
    private int _version;
    private bool _isActualVersionOfDiagnostics;
    private readonly List<Diagnostic> _diagnostics = [];
    private readonly List<Token> _unexpectedTokens = [];
    private readonly List<int> _lineLengths = [];
    private List<Token> _tokens = [];
    private List<BaseStatement> _statements = null!;
    private readonly DefinitionsMap _definedSymbols;
    private readonly IdentifierComparer _symbolComparer;
    public readonly TextDocumentAttributes Attributes;
    public readonly DocumentUri Uri;

    public Document(TextDocumentItem item)
    {
        Attributes = new TextDocumentAttributes(item.Uri, item.LanguageId);
        Uri = item.Uri;
        _version = item.Version ?? 0;
        _source = item.Text;
        _symbolComparer = new IdentifierComparer(_source);
        _definedSymbols = new DefinitionsMap(_symbolComparer);
        Initialize();
    }

    // TODO: remove after adding support for incremental updates
    private void Initialize()
    {
        MakeTokens();
        MakeStatements();
        MakeDefinitions();
    }

    private void MakeTokens()
    {
        _tokens.Clear();
        _lineLengths.Clear();
        new Lexer(_source, _lineLengths).ProcessUntilFileEnd(in _tokens);
    }

    private void MakeStatements()
    {
        BracketToken? ignored = null;
        _unexpectedTokens.Clear();
        var state = new ParsingState(_tokens, in _unexpectedTokens);
        _statements = BaseStatement.ParseSomeInBlock(state, ref ignored, isInner: false)
                      ?? [];
    }

    private void MakeDefinitions()
    {
        _definedSymbols.Clear();
        foreach (var child in _statements)
        {
            if (child is not ISymbolSource { symbol: { } symbol })
            {
                continue;
            }
            if (_definedSymbols.TryGetValue(symbol, out var equalSymbols))
            {
                equalSymbols.Add(symbol);
                continue;
            }
            _definedSymbols[symbol] = [symbol];
        }
    }
}