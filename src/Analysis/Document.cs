using NMLServer.Lexing;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing;
using NMLServer.Parsing.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NMLServer.Analysis;

internal class Document
{
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

            var line = change.Range.Start.Line;
            if (line > _lineLengths.Count - 1)
            {
                _lineLengths.EnsureCapacity(line + 1);
            }
            _lineLengths[line] = _source.Length - change.RangeLength + change.Text.Length;
            _source = _source[..change.Range.Start.Character] + change.Text + _source[change.Range.End.Character..];
            ++_version;
        }
        var lexer = new Lexer(_source);
        (_tokens, _lineLengths) = lexer.Process();
        _state = new ParsingState(_tokens);
        _file = new NMLFile(_state);
        _isActualVersionOfDiagnostics = false;
        Analyze();
    }

    private IEnumerable<Range> this[int start, int end]
    {
        // TODO: incorporate into semantic tokens method with keeping line/char numbers for faster finding
        // TODO  OR cache them here in case semantic tokens wouldn't be a single place to use this method
        get
        {
            /* Getting coordinates of starting position */
            int line = 0;
            int length = _lineLengths[line] + 1;
            while (start >= length)
            {
                start -= length;
                length = _lineLengths[++line] + 1;
            }
            var startLine = line;

            /* Getting coordinates of ending position */
            end -= start;
            while (end >= length)
            {
                end -= length;
                length = _lineLengths[++line] + 1;
            }
            var endChar = end;

            /* If on the same line */
            if (startLine == line)
            {
                yield return new Range(line, start, line, endChar);
                yield break;
            }

            yield return new Range(startLine, start, startLine, _lineLengths[startLine] - 1);
            for (var lineNum = startLine + 1; lineNum < line; ++lineNum)
            {
                yield return new Range(lineNum, 0, lineNum, _lineLengths[lineNum] - 1);
            }
            yield return new Range(line, 0, line, endChar);
        }
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
            _diagnostics.Add(
                new Diagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "Unexpected token",
                    Range = this[
                        unexpectedToken.Start,
                        unexpectedToken is MulticharToken hasEnd
                            ? hasEnd.End
                            : unexpectedToken.Start + 1
                    ].First() // TODO
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
            foreach (var range in this[token.Start, token.Start + length])
            {
                builder.Push(range, type);
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