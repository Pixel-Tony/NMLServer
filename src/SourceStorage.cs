using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;

namespace NMLServer;

internal class SourceStorage
{
    private readonly Dictionary<DocumentUri, Document> _documents = new();

    public Document this[DocumentUri uri]
        => _documents.TryGetValue(uri, out var result)
            ? result
            : throw new ArgumentException($"No document is present with given uri: {uri}");

    public Document GetDocument<T>(T identifier) where T : ITextDocumentIdentifierParams
        => this[identifier.TextDocument.Uri];

    public void Add(TextDocumentItem target)
    {
        _documents.Add(target.Uri, new Document(target));
        Program.Server.LogInfo($"Opened new file; Text - \"{target.Text[..20]}\"");
        // TODO: analyze document
    }

    public void Remove(TextDocumentIdentifier item) => _documents.Remove(item.Uri);

    public void ApplyChanges(DidChangeTextDocumentParams request)
    {
        var document = _documents[request.TextDocument.Uri];
        document.UpdateFrom(request);
        // TODO: analyze document
    }
}