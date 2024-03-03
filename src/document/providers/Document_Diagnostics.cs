using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal partial class Document
{
    // TODO: full -> incremental
    // TODO: +option "Max. number of problems (per file)"
    public List<Diagnostic> ProvideDiagnostics()
    {
        if (_isActualVersionOfDiagnostics)
        {
            return _diagnostics;
        }

        _diagnostics.Clear();
        _diagnostics.EnsureCapacity(_unexpectedTokens.Count);
        foreach (var unexpectedToken in _unexpectedTokens)
        {
            var length = unexpectedToken.length;
            var (line, @char) = LocalToProtocol(unexpectedToken.start);
            _diagnostics.Add(new Diagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = "Unexpected token",
                Range = new Range(line, @char, line, @char + length)
            });
        }
        _isActualVersionOfDiagnostics = true;
        return _diagnostics;
    }
}