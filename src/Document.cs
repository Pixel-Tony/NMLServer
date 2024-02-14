using System.Runtime.CompilerServices;
using System.Text;
using NMLServer.Lexing;
using NMLServer.Model.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NMLServer;

internal partial class Document
{
    public readonly DocumentUri Uri;
    private int _version;
    private bool _isActualVersionOfDiagnostics;
    private string _source;
    private readonly List<int> _lineLengths = new();
    private readonly List<Diagnostic> _diagnostics = new();

    private NMLFile _file;

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

        var tokens = Lexer.Process(_source, _lineLengths);
        var comparer = new IdentifierComparer(_source);
        _file = NMLFile.Make(tokens, comparer);
    }

    // TODO| find the list of statements affected by change, drop them;
    // TODO| use updated source range covered by them to get new sublist, insert into old list.
    public void UpdateFrom(DidChangeTextDocumentParams request)
    {
        if (_version >= request.TextDocument.Version)
        {
            return;
        }
        foreach (var change in request.ContentChanges)
        {
            if (change.Range is null)
            {
                _source = EncodeSource(change.Text);
                ++_version;
                continue;
            }
            var (startIndex, endIndex) = ProtocolToLocal(change.Range);
            var line = change.Range.Start.Line;
            if (line > _lineLengths.Count - 1)
            {
                _lineLengths.EnsureCapacity(line + 1);
            }
            _lineLengths[line] = _source.Length - change.RangeLength + change.Text.Length;
            {
                var span = _source.AsSpan();
                _source = string.Concat(span[..startIndex], EncodeSource(change.Text), span[endIndex..]);
            }
            ++_version;
        }

        var tokens = Lexer.Process(_source, _lineLengths);
        _file = NMLFile.Make(tokens, new IdentifierComparer(_source));
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

        // _file.ProvideDiagnostics(_diagnostics);

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
}