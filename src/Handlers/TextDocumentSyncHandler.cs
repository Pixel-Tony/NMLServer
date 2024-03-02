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
    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => storage[uri];

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken _)
    {
        Program.Server.LogInfo($"Added document {request.TextDocument.Uri}");
        storage.Add(request.TextDocument);
        return UnitTask.Result;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken _)
    {
        Program.Server.LogInfo($"Changed document {request.TextDocument.Uri}\n");
        storage.ApplyChanges(request);
        return UnitTask.Result;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken _)
    {
        Program.Server.LogInfo($"Saved document {request.TextDocument.Uri}\n");
        return UnitTask.Result;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken _)
    {
        Program.Server.LogInfo($"Removed document {request.TextDocument.Uri}\n");
        storage.Remove(request.TextDocument);
        return UnitTask.Result;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
        => _options;

    private static readonly TextDocumentSyncRegistrationOptions _options = new()
    {
        DocumentSelector = Program.NMLSelector,
        Change = TextDocumentSyncKind.Incremental,
        Save = new SaveOptions { IncludeText = false }
    };
}