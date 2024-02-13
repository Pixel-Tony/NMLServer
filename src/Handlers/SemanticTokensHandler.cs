using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer.Analysis;

internal class SemanticTokensHandler : SemanticTokensHandlerBase
{
    private readonly SourceStorage _storage;

    public SemanticTokensHandler(SourceStorage storage) => _storage = storage;

    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(
        SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
        => new()
        {
            DocumentSelector = Program.NMLSelector,
            Legend = new SemanticTokensLegend
            {
                TokenModifiers = capability.TokenModifiers,
                TokenTypes = capability.TokenTypes
            },
            Full = new SemanticTokensCapabilityRequestFull { Delta = false },
            Range = false
        };

    protected override Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken)
    {
        _storage.ProvideSemanticTokens(builder, identifier.TextDocument);
        return Task.CompletedTask;
    }

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams @params,
        CancellationToken _) => Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
}