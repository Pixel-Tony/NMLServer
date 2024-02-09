using System.Text;
using NMLServer.Lexing;
using NMLServer.Model;
using NMLServer.Model.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NMLServer;

internal class Document
{
    public const TextDocumentSyncKind SyncKind = TextDocumentSyncKind.Incremental;

    private ParsingState _state;
    public readonly DocumentUri Uri;
    private string _source;
    private IReadOnlyList<Token> _tokens;
    private List<int> _lineLengths;
    private readonly List<Diagnostic> _diagnostics = new();

    // TODO: propagate analysis to NMLFile and BaseStatement
    private NMLFile _file;
    private int _version;
    private bool _isActualVersionOfDiagnostics;

    public IEnumerable<Diagnostic> diagnostics => Analyze();

    private static string ProcessSource(string source)
    {
        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(source));
    }

    public Document(TextDocumentItem item)
    {
        Uri = item.Uri;
        _version = item.Version ?? 0;

        _source = item.Text;

        var lexer = new Lexer(ProcessSource(_source));
        (_tokens, _lineLengths) = lexer.Process();

        _state = new ParsingState(_tokens);
        _file = new NMLFile(_state);
    }

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
                _source = change.Text;
                ++_version;
                continue;
            }
            var (startIndex, endIndex) = GetCoordinates(change.Range);
            var line = change.Range.Start.Line;
            if (line > _lineLengths.Count - 1)
            {
                _lineLengths.EnsureCapacity(line + 1);
            }
            _lineLengths[line] = _source.Length - change.RangeLength + change.Text.Length;
            _source = _source[..startIndex] + change.Text + _source[endIndex..];
            ++_version;
        }
        var lexer = new Lexer(ProcessSource(_source));
        (_tokens, _lineLengths) = lexer.Process();
        _state = new ParsingState(_tokens);
        _file = new NMLFile(_state);
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
        int length = _lineLengths[line] + 1;
        while (start >= length)
        {
            start -= length;
            length = _lineLengths[++line] + 1;
        }
        return (line, start);
    }

    // TODO: incorporate into semantic tokens method with keeping line/char numbers for faster finding
    // TODO    OR cache them here in case semantic tokens wouldn't be a single place to use this method
    //
    // TODO: Optimise _lineLengths indexing
    /// <summary>
    /// For given token, convert its absolute position to sequence of ranges (line, character, length), that cover it.
    /// </summary>
    /// <param name="start">The index of first symbol of the token.</param>
    /// <param name="length">The length of the token.</param>
    /// <remarks>Given coordinates are such that <c>tokenContent = source[start..start + length]</c>.</remarks>
    private IEnumerable<(int, int, int)> GetRanges(int start, int length)
    {
        /* Getting coordinates of starting position */
        int line = 0;
        int lineLength = _lineLengths[line] + 1;
        while (start >= lineLength)
        {
            start -= lineLength;
            lineLength = _lineLengths[++line] + 1;
        }
        var startLine = line;
        /* length now contains shift from current 'character' of first position */
        length += start;
        /* If on the same line - return only one coordinates */
        if (length < lineLength)
        {
            return new[] { (line, start, length - start) };
        }
        /* Otherwise - return 'end piece' of first line, whole middle lines and 'start piece' of last line */
        /* Getting coordinates of ending position */
        while (length >= lineLength)
        {
            length -= lineLength;
            lineLength = _lineLengths[++line] + 1;
        }
        var result = new (int, int, int)[line - startLine + 1];

        int i = 0;
        result[i++] = (startLine, start, _lineLengths[startLine] - start);
        for (var lineNum = startLine + 1; lineNum < line; ++lineNum)
        {
            result[i++] = (lineNum, 0, _lineLengths[lineNum]);
        }
        result[i] = (line, 0, length);
        return result;
    }

    private (int start, int end) GetCoordinates(Range range)
    {
        var start = 0;
        int i = 0;
        while (i < range.Start.Line)
        {
            start += _lineLengths[i++] + 1;
        }
        var end = start;
        while (i < range.End.Line)
        {
            end += _lineLengths[i++] + 1;
        }
        return (start + range.Start.Character, end + range.End.Character);
    }

    private IEnumerable<Diagnostic> Analyze()
    {
        if (_isActualVersionOfDiagnostics)
        {
            return _diagnostics;
        }
        var unexpectedTokens = _state.unexpectedTokens;
        _diagnostics.Clear();
        _diagnostics.EnsureCapacity(unexpectedTokens.Count);

        foreach (var unexpectedToken in unexpectedTokens)
        {
            var start = unexpectedToken.Start;
            var length = GetTokenLength(unexpectedToken);
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

    public void ProvideSemanticTokens(SemanticTokensBuilder builder)
    {
        foreach (var token in _tokens)
        {
            var hasType = GetTokenType(token, out var type);
            var tokenLength = GetTokenLength(token);

            if (!hasType)
            {
                continue;
            }
            if (token is CommentToken)
            {
                /* only comments are allowed to span multiple lines */
                foreach (var (line, @char, length) in GetRanges(token.Start, tokenLength))
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

    private static bool GetTokenType(Token token, out SemanticTokenType? type)
    {
        return (type = token switch
        {
            CommentToken => SemanticTokenType.Comment,
            IdentifierToken id => id.kind.ToGeneralTokenType(),
            UnitToken
                or KeywordToken => SemanticTokenType.Keyword,
            NumericToken => SemanticTokenType.Number,
            StringToken => SemanticTokenType.String,
            ColonToken
                or RangeToken
                or BracketToken
                or UnaryOpToken
                or BinaryOpToken
                or TernaryOpToken
                or SemicolonToken
                or AssignmentToken
                => SemanticTokenType.Operator,
            _ => null
        }) is not null;
    }

    private static int GetTokenLength(Token token)
    {
        return token switch
        {
            BaseMulticharToken multichar => multichar.Length,
            UnitToken unitToken => unitToken.length,
            RangeToken => 2,
            _ => 1
        };
    }
}