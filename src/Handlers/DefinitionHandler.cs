using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer.Analysis;

internal class DefinitionHandler(SourceStorage storage) : DefinitionHandlerBase
{
    protected override DefinitionRegistrationOptions CreateRegistrationOptions(DefinitionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new DefinitionRegistrationOptions { DocumentSelector = Program.NMLSelector };
    }

    public override Task<LocationOrLocationLinks?> Handle(DefinitionParams request, CancellationToken _)
    {
        return Task.FromResult(storage.ProvideDefinition(request));
    }
}