using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

namespace NMLServer.Model.Processors.Diagnostics;

internal class DiagnosticContext
{
    private readonly List<DiagnosticTemplate> _templates = [];
    public List<Diagnostic> BuildDiagnostics(ref readonly TokenStorage storage)
    {
        _templates.Sort();
        var converter = storage.MakeConverter();
        // var backConverter = _storage.MakeConverter();
        // var frontConverter = backConverter.Copy();

        List<Diagnostic> result = new(_templates.Count);
        foreach (var (start, end, message, severity) in _templates)
        {
            var startPos = converter.LocalToProtocol(start);
            var endPos = end == start
                ? startPos with { Character = startPos.Character + 1 }
                : converter.Copy().LocalToProtocol(end);
            result.Add(new Diagnostic()
            {
                Range = new Range(startPos, endPos),
                Message = message,
                Severity = severity
            });
        }
        // var startPos = _converter.LocalToProtocol(start);
        // var endPos = _converter.LocalToProtocol(end);
        // Add(severity, message, new Range(startPos, endPos));

        // Position start = _converter.LocalToProtocol(offset);
        // Position end = new(start.Line, start.Character + 1);
        // Add(severity, message, new Range(start, end));
        return result;
    }

    public void Add<T>(string message, T item, DiagnosticSeverity severity = DiagnosticSeverity.Error) where T : IHasBounds
        => Add(message, item.Start, item.End, severity);

    public void Add(string message, int offset, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        => Add(message, offset, offset, severity);

    public void Add(string message, int start, int end, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        => _templates.Add(new DiagnosticTemplate(start, end, message, severity));

    private readonly record struct DiagnosticTemplate(int Start, int End, string Message, DiagnosticSeverity Severity)
        : IComparable<DiagnosticTemplate>
    {
        int IComparable<DiagnosticTemplate>.CompareTo(DiagnosticTemplate other) => (Start, End).CompareTo((other.Start, other.End));
    }
}