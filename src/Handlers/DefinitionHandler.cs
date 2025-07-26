using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace NMLServer.Handlers;

internal class DefinitionHandler(SourceStorage storage) : DefinitionHandlerBase
{
    protected override async Task<DefinitionResponse?> Handle(DefinitionParams p, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/definition <-");
        var document = storage[p.TextDocument.Uri];
        var locations = document.TryGetDefinitionLocations(p.Position);
        await Program.DebugAsync("textDocument/definition ->");
        return locations is null ? null : new DefinitionResponse(locations);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities _)
        => serverCapabilities.DefinitionProvider = true;
}