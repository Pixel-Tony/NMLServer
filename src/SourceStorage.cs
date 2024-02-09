using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal class SourceStorage
{
    public event Action<PublishDiagnosticsParams>? ShouldPublishDiagnostics;

    private readonly Dictionary<DocumentUri, (Document document, TextDocumentAttributes attributes)> _documents = new();

    public TextDocumentAttributes this[DocumentUri uri] => _documents[uri].attributes;

    public void Add(TextDocumentItem target)
    {
        if (_documents.ContainsKey(target.Uri))
        {
            throw new Exception("Specified document is already added");
        }
        var document = new Document(target);
        _documents[target.Uri] = (document, new TextDocumentAttributes(target.Uri, target.LanguageId));
        Analyze(document);
    }

    private void Analyze(Document document)
    {
        ShouldPublishDiagnostics?.Invoke(new PublishDiagnosticsParams
        {
            Uri = document.Uri,
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
        var document = _documents[request.TextDocument.Uri].document;
        document.UpdateFrom(request);
        Analyze(document);
    }

    public void ProvideSemanticTokens(SemanticTokensBuilder builder, TextDocumentIdentifier identifier)
    {
        _documents[identifier.Uri].document.ProvideSemanticTokens(builder);
    }

    public IEnumerable<Diagnostic> GetDiagnostics(TextDocumentIdentifier identifier)
    {
        return _documents[identifier.Uri].document.diagnostics;
    }
}