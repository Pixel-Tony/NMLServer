using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal class SourceStorage
{
    public event Action<PublishDiagnosticsParams>? ShouldPublishDiagnostics;

    private readonly Dictionary<DocumentUri, Document> _documents = new();

    public TextDocumentAttributes this[DocumentUri uri] => _documents[uri].Attributes;

    public void Add(TextDocumentItem target)
    {
        var document = new Document(target);
        _documents.Add(target.Uri, document);
        Analyze(document);
    }

    public void Remove(TextDocumentIdentifier item) => _documents.Remove(item.Uri);

    private void Analyze(Document document) => ShouldPublishDiagnostics?.Invoke(new PublishDiagnosticsParams
    {
        Uri = document.Uri,
        Diagnostics = document.ProvideDiagnostics()
    });

    public void ApplyChanges(DidChangeTextDocumentParams request)
    {
        var document = _documents[request.TextDocument.Uri];
        document.UpdateFrom(request);
        Analyze(document);
    }

    public void ProvideSemanticTokens(SemanticTokensBuilder builder, DocumentUri uri)
    {
        _documents[uri].ProvideSemanticTokens(builder);
    }

    public IEnumerable<Diagnostic> GetDiagnostics(TextDocumentIdentifier identifier)
    {
        return _documents[identifier.Uri].ProvideDiagnostics();
    }

    public LocationOrLocationLinks? ProvideDefinition(DefinitionParams request)
    {
        var document = _documents[request.TextDocument.Uri];
        return document.ProvideDefinitions(request.Position);
    }
}