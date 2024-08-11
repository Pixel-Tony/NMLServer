using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;

namespace NMLServer.Analysis;

internal class TextDocumentSyncHandler(SourceStorage storage) : TextDocumentSyncHandlerBase
{
    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => storage[uri].Attributes;

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken _)
    {
        storage.Add(request.TextDocument);
        Program.Server.LogInfo($"Opened file at {request.TextDocument.Uri}");
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken _)
    {
        storage.ApplyChanges(request);
        Program.Server.LogInfo($"Changed file at {request.TextDocument.Uri}");
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken _)
        => Unit.Task;

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken _)
    {
        storage.Remove(request.TextDocument);
        Program.Server.LogInfo($"Closed file at {request.TextDocument.Uri}");
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability, ClientCapabilities clientCapabilities) => new()
    {
        DocumentSelector = Program.NMLSelector,
        Change = TextDocumentSyncKind.Incremental
    };
}