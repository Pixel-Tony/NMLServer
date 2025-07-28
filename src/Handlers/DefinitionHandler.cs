using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model.Lexis;

namespace NMLServer.Handlers;

internal class DefinitionHandler(SourceStorage storage) : DefinitionHandlerBase
{
    protected override async Task<DefinitionResponse?> Handle(DefinitionParams p, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/definition <-");
        var uri = p.TextDocument.Uri;
        var document = storage[uri];
        var tokens = document.Tokens;

        if (tokens.TryGetAt(p.Position) is not IdentifierToken symbol)
            return null;

        if (!document.Has(symbol, out var definitions))
            return null;

        List<Location> locations = new(definitions.Count);
        var length = symbol.Length;
        var converter = document.Tokens.MakeConverter();
        foreach (var definition in definitions)
        {
            var definitionStart = definition.Start;
            var start = converter.LocalToProtocol(definitionStart);
            var end = start with { Character = start.Character + length };
            locations.Add(new Location(uri, new Range(start, end)));
        }
        await Program.DebugAsync("textDocument/definition ->");
        return locations is null ? null : new DefinitionResponse(locations);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities _)
        => serverCapabilities.DefinitionProvider = true;
}