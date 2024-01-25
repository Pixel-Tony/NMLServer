using NMLServer.Lexing;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing;
using NMLServer.Parsing.Statement;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NMLServer.Analysis;

internal class Document
{
    public TextDocumentItem Item;

    private DocumentPositionConverter _converter;

    // TODO: propagate semantic resolution to NMLFile and BaseStatement
    public NMLFile Context;
    private readonly List<Diagnostic> _diagnostics = new();
    private ParsingState _state;
    private (Token[] tokens, CommentToken[] comments) _lexed;
    private bool isActualVersionOfDiagnostics { get; set; }

    public IEnumerable<Diagnostic> diagnostics => Analyze();

    public Document(TextDocumentItem item)
    {
        Item = item;
        var source = item.Text;
        _converter = new DocumentPositionConverter(source);
        var lexer = new Lexer(source);
        _lexed = lexer.Process();
        _state = new ParsingState(_lexed.tokens);
        Context = new NMLFile(_state);
        Analyze();
    }

    private IEnumerable<Diagnostic> Analyze()
    {
        if (isActualVersionOfDiagnostics)
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
                    Range = new Range
                    {
                        Start = _converter[unexpectedToken.Start],
                        End = _converter[unexpectedToken is MulticharToken hasEnd
                            ? hasEnd.End
                            : unexpectedToken.Start + 1]
                    }
                }
            );
        }
        isActualVersionOfDiagnostics = true;
        return _diagnostics;
    }

    public void ApplyChanges(DidChangeTextDocumentParams request)
    {
        if (Item.Version >= request.TextDocument.Version)
        {
            return;
        }
        string source = Item.Text;
        int version = Item.Version ?? 0;
        foreach (var change in request.ContentChanges)
        {
            source = change.Text;
            ++version;
        }
        Item = new TextDocumentItem
        {
            Text = source,
            Uri = Item.Uri,
            Version = version,
            LanguageId = Item.LanguageId
        };

        _converter = new DocumentPositionConverter(source);
        var lexer = new Lexer(source);
        _lexed = lexer.Process();
        _state = new ParsingState(_lexed.tokens);
        Context = new NMLFile(_state);

        isActualVersionOfDiagnostics = false;
        Analyze();
    }

    public void ProvideSemanticTokens(SemanticTokensBuilder builder)
    {
        string context = Item.Text;

        var tokens = _lexed.tokens.ToList();
        tokens.AddRange(_lexed.comments);
        tokens.Sort((left, right) => left.Start.CompareTo(right.Start));

        foreach (var token in tokens)
        {
            var (type, length) = GetTokenTypeAndLength(token, context);
            foreach (var range in _converter[_converter[token.Start], _converter[token.Start + length]])
            {
                builder.Push(range, type);
            }
        }
    }

    private static (SemanticTokenType? type, int length) GetTokenTypeAndLength(Token token, string context)
    {
        var type = token switch
        {
            AssignmentToken => SemanticTokenType.Operator,
            BinaryOpToken => SemanticTokenType.Operator,
            BracketToken => SemanticTokenType.Operator,
            ColonToken => SemanticTokenType.Operator,
            CommentToken => SemanticTokenType.Comment,
            FailedToken => null as SemanticTokenType?,
            IdentifierToken id => DecideTokenType(id, context),
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

    // TODO: add initial token type for known identifiers at lexing state to avoid leaking dependencies
    private static SemanticTokenType DecideTokenType(MulticharToken token, string context)
    {

        var s = context[token.Start..token.End];
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