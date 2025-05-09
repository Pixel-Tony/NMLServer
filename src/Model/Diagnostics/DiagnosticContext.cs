using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Diagnostics;

internal readonly ref struct DiagnosticContext(ref Document.PositionConverter converter)
{
    private readonly ref Document.PositionConverter _converter = ref converter;
    public readonly List<Diagnostic> Diagnostics = [];

    public void AddError<T>(string message, T location)
        where T : IHasStart, IHasEnd
        => AddError(message, location.start, location.end);

    public void AddError(string message, int offset)
    {
        Position start = _converter.LocalToProtocol(offset);
        Position end = new(start.Line, start.Character + 1);
        Add(DiagnosticSeverity.Error, message, new Range(start, end));
    }

    public void AddError(string message, int start, int end)
    {
        var startPos = _converter.LocalToProtocol(start);
        var endPos = _converter.LocalToProtocol(end);
        Add(DiagnosticSeverity.Error, message, new Range(startPos, endPos));
    }

    private void Add(DiagnosticSeverity severity, string message, Range range)
        => Diagnostics.Add(new Diagnostic { Severity = severity, Message = message, Range = range });
}