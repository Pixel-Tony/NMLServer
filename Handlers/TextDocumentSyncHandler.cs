using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.PublishDiagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Logging;
using NMLServer.Model;

namespace NMLServer.Handlers;

internal class TextDocumentSyncHandler(SourceStorage storage) : TextDocumentHandlerBase
{
    private static async Task PublishDiagnostics(Document doc)
    {
        var content = doc.Diagnostics.Content;
        PublishDiagnosticsParams response = new() { Diagnostics = content, Uri = doc.Uri, Version = doc.Version };
        await Program.Server.Client.PublishDiagnostics(response).ConfigureAwait(false);
    }

    protected override async Task Handle(DidOpenTextDocumentParams p, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/didOpen <-");
        var item = p.TextDocument;
        try
        {
            Document doc = new(item);
            storage[item.Uri] = doc;
            await PublishDiagnostics(doc);
            await Logger.DebugAsync("textDocument/didOpen ->");
        }
        catch (Exception e)
        {
            Logger.Debug(e);
            throw;
        }
    }

    protected override async Task Handle(DidChangeTextDocumentParams p, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/didChange <-");
        try
        {
            var doc = storage[p.TextDocument.Uri];
            doc.AcceptChanges(p.TextDocument.Version, p.ContentChanges);
            await PublishDiagnostics(doc);
            await Logger.DebugAsync("textDocument/didChange ->");
        }
        catch (Exception e)
        {
            Logger.Debug(e);
            throw;
        }
    }

    protected override Task Handle(WillSaveTextDocumentParams p, CancellationToken _) => Task.CompletedTask;

    protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams p, CancellationToken _)
        => Task.FromResult<List<TextEdit>?>(null);

    protected override async Task Handle(DidCloseTextDocumentParams p, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/didClose <-");
        storage.Remove(p.TextDocument.Uri);
        await Logger.DebugAsync("textDocument/didClose ->");
    }

    public override void RegisterCapability(ServerCapabilities server, ClientCapabilities _)
    {
        server.TextDocumentSync = new TextDocumentSyncOptions
        {
            Change = TextDocumentSyncKind.Incremental,
            OpenClose = true
        };
    }
}