using NMLServer.Lexing;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing;
using NMLServer.Parsing.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NMLServer.Analysis;

internal class Document
{
    public const TextDocumentSyncKind SyncKind = TextDocumentSyncKind.Incremental;

    private ParsingState _state;
    public readonly DocumentUri Uri;
    private string _source;
    private IReadOnlyList<Token> _tokens;
    private List<int> _lineLengths;
    private readonly List<Diagnostic> _diagnostics = new();

    public string languageId { get; }

    // TODO: propagate analysis to NMLFile and BaseStatement
    private NMLFile _file;
    private int _version;
    private bool _isActualVersionOfDiagnostics;

    public IEnumerable<Diagnostic> diagnostics => Analyze();

    public Document(TextDocumentItem item)
    {
        Uri = item.Uri;
        _version = item.Version ?? 0;
        languageId = item.LanguageId;

        _source = item.Text;

        var lexer = new Lexer(_source);
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
        var lexer = new Lexer(_source);
        (_tokens, _lineLengths) = lexer.Process();
        _state = new ParsingState(_tokens);
        _file = new NMLFile(_state);
        _isActualVersionOfDiagnostics = false;
        Analyze();
    }

    /// <summary>
    /// Convert two edge positions of token to Range, assuming that given token spans over single line.
    /// </summary>
    /// <param name="start">The index of first token symbol.</param>
    /// <param name="end">The index of symbol after the token.</param>
    /// <remarks>Given coordinates are such that <c>tokenContent = source[start..end]</c>.</remarks>
    /// <returns>The Range representation of passed source span.</returns>
    private Range GetRange(int start, int end)
    {
        /* Get relative position of end to start; end now contains length of the token. */
        end -= start;

        /* Getting coordinates of starting position */
        int line = 0;
        int length = _lineLengths[line] + 1;
        while (start >= length)
        {
            start -= length;
            length = _lineLengths[++line] + 1;
        }
        return new Range(line, start, line, end + start);
    }

    // TODO: incorporate into semantic tokens method with keeping line/char numbers for faster finding
    // TODO    OR cache them here in case semantic tokens wouldn't be a single place to use this method
    //
    // TODO: Optimise _lineLengths indexing
    /// <summary>
    /// Convert two edge positions of token to enumerable of Ranges, suitable for passing to LSP methods.
    /// </summary>
    /// <param name="start">The index of first token symbol.</param>
    /// <param name="end">The index of symbol after the token.</param>
    /// <remarks>Given coordinates are such that <c>tokenContent = source[start..end]</c>.</remarks>
    private IEnumerable<Range> GetRanges(int start, int end)
    {
        /* Get relative position of end to start; end now contains length of the token. */
        end -= start;

        /* Getting coordinates of starting position */
        int line = 0;
        int length = _lineLengths[line] + 1;
        while (start >= length)
        {
            start -= length;
            length = _lineLengths[++line] + 1;
        }
        var startLine = line;

        /* end now contains shift from current 'character' of first position */
        end += start;

        /* Getting coordinates of ending position */
        while (end >= length)
        {
            end -= length;
            length = _lineLengths[++line] + 1;
        }

        /* If on the same line - yield only one Range */
        if (startLine == line)
        {
            yield return new Range(line, start, line, end);
            yield break;
        }

        /* Otherwise - yield 'end piece' of first line, whole middle lines, 'start piece' of last line */
        yield return new Range(startLine, start, startLine, _lineLengths[startLine]);
        for (var lineNum = startLine + 1; lineNum < line; ++lineNum)
        {
            yield return new Range(lineNum, 0, lineNum, _lineLengths[lineNum]);
        }
        yield return new Range(line, 0, line, end);
    }

    private (int start, int end) GetCoordinates(Range range)
    {
        var start = 0;
        for (int i = 0; i < range.Start.Line; ++i)
        {
            start += _lineLengths[i] + 1;
        }
        var end = start;
        for (int i = range.Start.Line; i < range.End.Line; ++i)
        {
            end += _lineLengths[i] + 1;
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
            var end = start + GetTokenTypeAndLength(unexpectedToken).length;

            _diagnostics.Add(
                new Diagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "Unexpected token",
                    Range = GetRange(start, end)
                }
            );
        }
        _isActualVersionOfDiagnostics = true;
        return _diagnostics;
    }

    public void ProvideSemanticTokens(SemanticTokensBuilder builder)
    {
        foreach (var token in _tokens)
        {
            var (type, length) = GetTokenTypeAndLength(token);
            if (token is CommentToken)
            {
                foreach (var range in GetRanges(token.Start, token.Start + length))
                {
                    builder.Push(range, type);
                }
            }
            else
            {
                builder.Push(GetRange(token.Start, token.Start + length), type);
            }
        }
    }

    private (SemanticTokenType? type, int length) GetTokenTypeAndLength(Token token)
    {
        var type = token switch
        {
            AssignmentToken => SemanticTokenType.Operator,
            BinaryOpToken => SemanticTokenType.Operator,
            BracketToken => SemanticTokenType.Operator,
            ColonToken => SemanticTokenType.Operator,
            CommentToken => SemanticTokenType.Comment,
            FailedToken => null as SemanticTokenType?,
            IdentifierToken id => DecideTokenType(id),
            KeywordToken => SemanticTokenType.Keyword,
            NumericToken => SemanticTokenType.Number,
            StringToken => SemanticTokenType.String,
            RangeToken => SemanticTokenType.Operator,
            SemicolonToken => SemanticTokenType.Operator,
            TernaryOpToken => SemanticTokenType.Operator,
            UnaryOpToken => SemanticTokenType.Operator,
            UnitToken => SemanticTokenType.Keyword,
            _ => throw new ArgumentOutOfRangeException(nameof(token), "Unexpected token type")
        };
        var length = token switch
        {
            MulticharToken multichar => multichar.End - multichar.Start,
            UnitToken unitToken => unitToken.length,
            RangeToken => 2,
            _ => 1
        };

        return (type, length);
    }

    // TODO: in future, add initial token type for known identifiers at lexing state
    private SemanticTokenType DecideTokenType(MulticharToken token)
    {
        var s = _source[token.Start..token.End];
        if (Grammar.FeatureIdentifiers.Contains(s))
        {
            return SemanticTokenType.Type;
        }
        if (Grammar.FunctionIdentifiers.Contains(s))
        {
            return SemanticTokenType.Function;
        }
        return Grammar.Constants.Contains(s)
            ? SemanticTokenType.EnumMember
            : SemanticTokenType.Variable;
    }
}