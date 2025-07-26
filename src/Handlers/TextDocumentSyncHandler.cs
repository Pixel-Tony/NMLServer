using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.PublishDiagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model;

namespace NMLServer.Handlers;

internal class TextDocumentSyncHandler(SourceStorage storage) : TextDocumentHandlerBase
{
    private static async Task PublishDiagnostics(Document doc)
    {
        var content = doc.ProvideDiagnostics();
        PublishDiagnosticsParams response = new() { Diagnostics = content, Uri = doc.Uri, Version = doc.Version };
        await Program.Server.Client.PublishDiagnostics(response).ConfigureAwait(false);
    }

    protected override async Task Handle(DidOpenTextDocumentParams p, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/didOpen <-");
        var item = p.TextDocument;
        Document doc = new(item);
        storage[item.Uri] = doc;
        await PublishDiagnostics(doc);
        await Program.DebugAsync("textDocument/didOpen ->");
    }

    protected override async Task Handle(DidChangeTextDocumentParams p, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/didChange <-");
        var doc = storage[p.TextDocument.Uri];
        doc.Handle(p);
        await PublishDiagnostics(doc);
        await Program.DebugAsync("textDocument/didChange ->");
    }

    protected override Task Handle(WillSaveTextDocumentParams p, CancellationToken _) => Task.CompletedTask;

    protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams p, CancellationToken _)
        => Task.FromResult<List<TextEdit>?>(null);

    protected override async Task Handle(DidCloseTextDocumentParams p, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/didClose <-");
#if TREE_VISUALIZER_ENABLED
        if (storage.Remove(p.TextDocument.Uri, out var item))
            item.Dispose();
#else
        storage.Remove(p.TextDocument.Uri);
#endif
        await Program.DebugAsync("textDocument/didClose ->");
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