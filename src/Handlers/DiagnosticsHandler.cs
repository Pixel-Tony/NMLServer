using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentDiagnostic;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace NMLServer.Handlers;

internal class DiagnosticsHandler(SourceStorage storage) : DocumentDiagnosticHandlerBase
{
    protected override async Task<DocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/diagnostic <-");
        var doc = storage[request.TextDocument.Uri];
        var diagnostics = doc.ProvideDiagnostics();
        RelatedFullDocumentDiagnosticReport result = new() { Diagnostics = diagnostics };
        await Program.DebugAsync("textDocument/diagnostic ->");
        return result;
    }

    public override void RegisterCapability(ServerCapabilities server, ClientCapabilities _)
    {
        server.DiagnosticProvider = new DiagnosticOptions();
    }
}