using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer.Analysis;

internal class DocumentDiagnosticHandler(SourceStorage storage) : DocumentDiagnosticHandlerBase
{
    protected override DiagnosticsRegistrationOptions CreateRegistrationOptions(DiagnosticClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new DiagnosticsRegistrationOptions { DocumentSelector = Program.NMLSelector };
    }

    public override Task<RelatedDocumentDiagnosticReport> Handle(DocumentDiagnosticParams request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<RelatedDocumentDiagnosticReport>(new RelatedFullDocumentDiagnosticReport
        {
            Items = new Container<Diagnostic>(storage.GetDiagnostics(request.TextDocument))
        });
    }
}