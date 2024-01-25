using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace NMLServer.Analysis;

internal class SourceStorage
{
    public const TextDocumentSyncKind SyncKind = TextDocumentSyncKind.Full;
    public event Action<PublishDiagnosticsParams>? ShouldPublishDiagnostics;

    private readonly Dictionary<DocumentUri, Document> _documents = new();

    public TextDocumentAttributes this[DocumentUri uri] => new(uri, _documents[uri].Item.LanguageId);

    public void Add(TextDocumentItem target)
    {
        if (_documents.TryGetValue(target.Uri, out _))
        {
            throw new Exception("Specified document is already added");
        }
        var document = new Document(target);
        _documents[target.Uri] = document;
        Analyze(document);
    }

    private void Analyze(Document document)
    {
        ShouldPublishDiagnostics?.Invoke(new PublishDiagnosticsParams
        {
            Uri = document.Item.Uri,
            Diagnostics = new Container<Diagnostic>(document.diagnostics)
        });
    }

    public void Remove(TextDocumentIdentifier item)
    {
        if (!_documents.ContainsKey(item.Uri))
        {
            throw new Exception("Specified document not present.");
        }
        _documents.Remove(item.Uri);
    }

    public void ApplyChanges(DidChangeTextDocumentParams request)
    {
        var target = _documents[request.TextDocument.Uri];
        target.ApplyChanges(request);
        Analyze(target);
    }

    public void ProvideSemanticTokens(SemanticTokensBuilder builder, TextDocumentIdentifier identifier)
    {
        _documents[identifier.Uri].ProvideSemanticTokens(builder);
    }

    public IEnumerable<Diagnostic> GetActualDiagnostics(TextDocumentIdentifier identifier)
    {
        return _documents[identifier.Uri].diagnostics;
    }
}