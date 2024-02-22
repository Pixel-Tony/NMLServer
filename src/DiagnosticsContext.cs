using NMLServer.Model.Expression;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal class DiagnosticsContext
{
    private readonly List<(string Message, DiagnosticSeverity Severity, int Start, int Length)> _diagnostics = new();

    public IEnumerable<Diagnostic> GetDiagnostics(Document document)
    {
        List<Diagnostic> result = new(_diagnostics.Count);
        foreach (var (message, diagnosticSeverity, start, length) in _diagnostics)
        {
            foreach (var (line, @char, rangeLength) in document.LocalToProtocol(start, length))
            {
                result.Add(new Diagnostic
                {
                    Range = new Range(line, @char, line, @char + rangeLength),
                    Severity = diagnosticSeverity,
                    Message = message
                });
            }
        }
        return result;
    }

    private void Add(string message, DiagnosticSeverity severity, int start, int length = 0)
        => _diagnostics.Add((message, severity, start, length));

    private void Add(string message, DiagnosticSeverity severity, ExpressionAST target)
        => Add(message, severity, target.start, target.end - target.start);

    public void AddError(string message, ExpressionAST target)
        => Add(message, DiagnosticSeverity.Error, target);

    public void AddError(string message, int start, int length = 0)
        => Add(message, DiagnosticSeverity.Error, start, length);
}