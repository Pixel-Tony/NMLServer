using System.Runtime.CompilerServices;
using System.Text;
using NMLServer.Lexing;
using NMLServer.Model;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NMLServer;

internal partial class Document
{
    public readonly DocumentUri Uri;
    private readonly List<int> _lineLengths = new();
    private readonly List<Diagnostic> _diagnostics = new();

    private string _source;

    private string source
    {
        set => _symbolComparer.Context = _source = value;
    }

    private readonly IdentifierComparer _symbolComparer;

    private NMLFile _file;
    private int _version;
    private bool _isActualVersionOfDiagnostics;

    public IEnumerable<Diagnostic> diagnostics => Analyze();

    private static string EncodeSource(string source)
    {
        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(source));
    }

    public Document(TextDocumentItem item)
    {
        Uri = item.Uri;
        _version = item.Version ?? 0;
        _source = EncodeSource(item.Text);
        _symbolComparer = new IdentifierComparer(_source);

        var tokens = Lexer.Process(_source, in _lineLengths);
        _file = new NMLFile(tokens, _symbolComparer);
    }

    // TODO find the list of statements affected by change, drop them and cleanup;
    // TODO use updated source range covered by them to get new sublist, insert into old list.
    public void UpdateFrom(DidChangeTextDocumentParams request)
    {
        if (_version >= request.TextDocument.Version)
        {
            return;
        }
        foreach (var change in request.ContentChanges)
        {
            var range = change.Range;
            if (range is null)
            {
                source = EncodeSource(change.Text);
                ++_version;
                continue;
            }
            var (startIndex, endIndex) = ProtocolToLocal(range);
            var line = range.Start.Line;
            var text = EncodeSource(change.Text);
            _lineLengths.EnsureCapacity(line + 1);
            _lineLengths[line] = _source.Length - change.RangeLength + text.Length;
            var span = _source.AsSpan();
            source = string.Concat(span[..startIndex], text, span[endIndex..]);
            ++_version;
        }

        var tokens = Lexer.Process(_source, in _lineLengths);
        _file = new NMLFile(tokens, _symbolComparer);
        _isActualVersionOfDiagnostics = false;
        Analyze();
    }

    /// <summary>
    /// Convert start position of token to position in file, assuming that given token spans over single line.
    /// </summary>
    /// <param name="start">The index of first token symbol.</param>
    /// <returns>The two-coordinate representation of passed source position.</returns>
    private (int line, int @char) GetPosition(int start)
    {
        int line = 0;
        int length = _lineLengths[line];
        while (start >= length)
        {
            start -= length;
            length = _lineLengths[++line];
        }
        return (line, start);
    }

    // TODO: incorporate into methods with multiple calls for caching line/char numbers and thus faster execution
    //       OR cache them here in case semantic tokens wouldn't be a single place to use this method
    /// <summary>
    /// For given token, convert its absolute position to sequence of ranges (line, character, length), that cover it.
    /// </summary>
    /// <param name="start">The index of first symbol of the token.</param>
    /// <param name="length">The length of the token.</param>
    /// <remarks>Given coordinates are such that <c>tokenContent = source[start..start + length]</c>.</remarks>
    public IEnumerable<(int line, int @char, int length)> LocalToProtocol(int start, int length)
    {
        /* Getting coordinates of starting position */
        int line = 0;
        int lineLength = _lineLengths[line];
        while (start >= lineLength)
        {
            start -= lineLength;
            lineLength = _lineLengths[++line];
        }
        var startLine = line;
        /* length now contains shift from first position line's character index */
        length += start;
        /* If on the same line - return only one coordinates */
        if (length <= lineLength)
        {
            return new[] { (line, start, length - start) };
        }
        /* Otherwise - return 'end piece' of first line, whole middle lines and 'start piece' of last line.
         * Getting coordinates of ending position
         */
        while (length >= lineLength)
        {
            length -= lineLength;
            lineLength = _lineLengths[++line];
        }
        var result = new (int, int, int)[line - startLine + 1];

        result[0] = (startLine, start, _lineLengths[startLine] - start);
        for (int i = 1, lineNum = startLine + 1; lineNum < line; ++lineNum)
        {
            result[i++] = (lineNum, 0, _lineLengths[lineNum]);
        }
        result[line] = (line, 0, length);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ProtocolToLocal(Position position)
    {
        var start = position.Character;
        for (int i = 0; i < position.Line; ++i)
        {
            start += _lineLengths[i];
        }
        return start;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (int start, int end) ProtocolToLocal(Range range)
    {
        var start = 0;
        int i = 0;
        while (i < range.Start.Line)
        {
            start += _lineLengths[i++];
        }
        var end = start;
        while (i < range.End.Line)
        {
            end += _lineLengths[i++];
        }
        return (start + range.Start.Character, end + range.End.Character);
    }

    // TODO: add option "max. number of problems per file"
    // TODO: full to delta, if possible
    private IEnumerable<Diagnostic> Analyze()
    {
        if (_isActualVersionOfDiagnostics)
        {
            return _diagnostics;
        }
        var unexpectedTokens = _file.unexpectedTokens;
        _diagnostics.Clear();
        _diagnostics.EnsureCapacity(unexpectedTokens.Count);

        foreach (var unexpectedToken in unexpectedTokens)
        {
            var start = unexpectedToken.Start;
            var length = unexpectedToken.GetLength();
            var (line, @char) = GetPosition(start);
            _diagnostics.Add(new Diagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = "Unexpected token",
                Range = new Range(line, @char, line, @char + length),
            });
        }
        _diagnostics.AddRange(_file.ProvideDiagnostics(this));
        _isActualVersionOfDiagnostics = true;
        return _diagnostics;
    }

    // TODO: full to delta, if possible
    public void ProvideSemanticTokens(SemanticTokensBuilder builder)
    {
        foreach (var token in _file.tokens)
        {
            var hasType = Grammar.GetTokenSemanticType(token, out var type);
            var tokenLength = token.GetLength();

            if (!hasType)
            {
                continue;
            }
            if (token is CommentToken)
            {
                /* only comments are allowed to span multiple lines */
                foreach (var (line, @char, length) in LocalToProtocol(token.Start, tokenLength))
                {
                    builder.Push(line, @char, length, type);
                }
                continue;
            }
            {
                var (line, @char) = GetPosition(token.Start);
                builder.Push(line, @char, tokenLength, type);
            }
        }
    }

    public LocationOrLocationLinks? ProvideDefinition(Position position)
    {
        if (_file.isEmpty)
        {
            return null;
        }
        var coordinate = ProtocolToLocal(position);
        var token = _file.GetTokenAt(coordinate);
        if (token is not IdentifierToken { Length: var tokenLength } symbol)
        {
            return null;
        }
        var elements = _file.GetSourcesForSymbol(symbol);
        if (elements is null)
        {
            return null;
        }
        List<LocationOrLocationLink> locations = new(capacity: elements.Count);
        foreach (var sameSymbol in elements)
        {
            var (line, @char) = GetPosition(sameSymbol.Start);
            locations.Add(new Location
            {
                Uri = Uri,
                Range = new Range(line, @char, line, @char + tokenLength)
            });
        }
        return locations;
    }
}