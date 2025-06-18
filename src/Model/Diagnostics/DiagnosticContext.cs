using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Diagnostics;

internal readonly ref struct DiagnosticContext(ref TokenStorage.PositionConverter converter)
{
    private readonly ref TokenStorage.PositionConverter _converter = ref converter;
    public readonly List<Diagnostic> Diagnostics = [];

    public void Add<T>(string message, T item, DiagnosticSeverity severity = DiagnosticSeverity.Error)
        where T : IHasStart, IHasEnd
        => Add(message, item.start, item.end, severity);

    public void Add(string message, int offset, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        Position start = _converter.LocalToProtocol(offset);
        Position end = new(start.Line, start.Character + 1);
        Add(severity, message, new Range(start, end));
    }

    public void Add(string message, int start, int end, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        var startPos = _converter.LocalToProtocol(start);
        var endPos = _converter.LocalToProtocol(end);
        Add(severity, message, new Range(startPos, endPos));
    }

    private void Add(DiagnosticSeverity severity, string message, Range range)
        => Diagnostics.Add(new Diagnostic { Severity = severity, Message = message, Range = range });
}