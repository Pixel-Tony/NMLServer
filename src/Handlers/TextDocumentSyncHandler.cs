using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace NMLServer.Analysis;

internal class TextDocumentSyncHandler(SourceStorage storage) : TextDocumentSyncHandlerBase
{
    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => storage[uri];

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken _)
    {
        storage.Add(request.TextDocument);
        return UnitTask.Result;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken _)
    {
        storage.ApplyChanges(request);
        return UnitTask.Result;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken _)
    {
        return UnitTask.Result;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken _)
    {
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