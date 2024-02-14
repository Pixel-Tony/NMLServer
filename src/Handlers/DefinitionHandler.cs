using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer.Analysis;

internal class DefinitionHandler : DefinitionHandlerBase
{
    private readonly SourceStorage _storage;

    public DefinitionHandler(SourceStorage storage)
    {
        _storage = storage;
    }

    protected override DefinitionRegistrationOptions CreateRegistrationOptions(DefinitionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new DefinitionRegistrationOptions { DocumentSelector = Program.NMLSelector };
    }

    public override Task<LocationOrLocationLinks?> Handle(DefinitionParams request, CancellationToken _)
    {
        return Task.FromResult(_storage.ProvideDefinition(request));
    }
}