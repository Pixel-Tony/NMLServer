using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentDiagnostic;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Logging;

namespace NMLServer.Handlers;

internal class DiagnosticsHandler(SourceStorage storage) : DocumentDiagnosticHandlerBase
{
    protected override async Task<DocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/diagnostic <-/->");
        return new RelatedFullDocumentDiagnosticReport()
        {
            Diagnostics = storage[request.TextDocument.Uri].Diagnostics.Content
        };
    }

    public override void RegisterCapability(ServerCapabilities server, ClientCapabilities _)
    {
        server.DiagnosticProvider = new DiagnosticOptions();
    }
}