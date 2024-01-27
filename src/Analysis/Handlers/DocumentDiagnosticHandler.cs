using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer.Analysis;

internal class DocumentDiagnosticHandler : DocumentDiagnosticHandlerBase
{
    private readonly SourceStorage _storage;

    public DocumentDiagnosticHandler(SourceStorage storage)
    {
        _storage = storage;
    }

    protected override DiagnosticsRegistrationOptions CreateRegistrationOptions(DiagnosticClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new DiagnosticsRegistrationOptions { DocumentSelector = TextDocumentSelector.ForLanguage("nml") };
    }

    public override Task<RelatedDocumentDiagnosticReport> Handle(DocumentDiagnosticParams request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<RelatedDocumentDiagnosticReport>(new RelatedFullDocumentDiagnosticReport
        {
            Items = new Container<Diagnostic>(_storage.GetDiagnostics(request.TextDocument))
        });
    }
}