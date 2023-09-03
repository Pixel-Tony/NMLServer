using LanguageServer.Parameters;
using LanguageServer.Parameters.TextDocument;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Results;
using LangRange = LanguageServer.Parameters.Range;

namespace NMLServer.Parsing.Statement;

internal class GRFBlock : BaseTitledDeclaration
{
    private BracketToken? _openingBracket;
    private NMLAttribute[]? _attributes;
    private GRFParameter[]? _parameters;
    private BracketToken? _closingBracket;

    private GRFBlock(NMLFileRoot? parent, KeywordToken alwaysGRF) : base(parent, alwaysGRF)
    { }

    public void Analyze(IList<Diagnostic> diagnostics, IPositionConverter converter, string s)
    {
        if (_openingBracket is null)
        {
            var pos = converter[Type.End];
            var pos2 = converter[Type.End + 1];
            diagnostics.Add(new Diagnostic
            {
                severity = DiagnosticSeverity.Error,
                message = "Missing opening bracket",
                range = new LangRange { start = pos, end = pos2 }
            });
        }

        if (_closingBracket is null)
        {
            var pos = converter[(_openingBracket?.Start ?? Type.End - 1) + 1];
            diagnostics.Add(new Diagnostic
            {
                severity = DiagnosticSeverity.Error,
                message = "Missing closing bracket",
                range = new LangRange { start = pos, end = pos }
            });
        }
    }

    // TODO
    public static GRFBlock BuildFromParseResult(NMLFileRoot? parent, BlockStatementParseResult result, string textualContext)
    {
        var output = new GRFBlock(parent, (result.Keyword as KeywordToken)!)
        {
            _openingBracket = result.Body.OpeningBracket,
            _attributes = result.Body.Attributes.Count > 0
                ? result.Body.Attributes.ToArray()
                : null,
            _closingBracket = result.Body.ClosingBracket,
        };

        List<GRFParameter> parsedParameters = new();
        foreach (var block in result.Body.Blocks)
        {
            var value = GRFParameter.FromParseResult(in output, block, textualContext);
            if (value != null)
            {
                parsedParameters.Add(value);
            }
        }
        output._parameters = parsedParameters.Count > 0
            ? parsedParameters.ToArray()
            : null;

        return output;
    }

    public override string ToString()
    {
        return $"{Type.Value} {_openingBracket?.Bracket ?? '.'}\n"
               + $"  {(_attributes != null ? string.Join("\n", _attributes) : "")}\n"
               + $"  {(_parameters != null ? string.Join("\n", _parameters as IEnumerable<GRFParameter>) : "")}\n"
               + $"{_closingBracket?.Bracket ?? '.'}";
    }
}