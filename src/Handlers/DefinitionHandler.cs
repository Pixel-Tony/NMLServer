using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace NMLServer.Handlers;

internal class DefinitionHandler(SourceStorage storage) : DefinitionHandlerBase
{
    protected override Task<DefinitionResponse?> Handle(DefinitionParams p, CancellationToken _)
    {
        var document = storage[p.TextDocument.Uri];
        var locations = document.TryGetDefinitionLocations(p.Position);
        return Task.FromResult(locations is null ? null : new DefinitionResponse(locations));
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities _)
        => serverCapabilities.DefinitionProvider = true;
}