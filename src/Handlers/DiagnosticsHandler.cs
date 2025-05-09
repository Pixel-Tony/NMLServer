using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentDiagnostic;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace NMLServer.Handlers;

internal class DiagnosticsHandler(SourceStorage storage) : DocumentDiagnosticHandlerBase
{
    protected override Task<DocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken _)
    {
        var doc = storage[request.TextDocument.Uri];
        return Task.FromResult<DocumentDiagnosticReport>(
            new RelatedFullDocumentDiagnosticReport { Diagnostics = doc.ProvideDiagnostics() }
        );
    }

    public override void RegisterCapability(ServerCapabilities server, ClientCapabilities _)
    {
        server.DiagnosticProvider = new DiagnosticOptions();
    }
}