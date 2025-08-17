using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Logging;
using NMLServer.Model.Tokens;

namespace NMLServer.Handlers;

internal class DefinitionHandler(SourceStorage storage) : DefinitionHandlerBase
{
    protected override async Task<DefinitionResponse?> Handle(DefinitionParams p, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/definition <-");
        var uri = p.TextDocument.Uri;
        var document = storage[uri];
        var symbols = document.Definitions.Symbols;
        ref readonly var tokens = ref document.AST.Tokens;
        if (tokens.At(p.Position) is not IdentifierToken symbol)
            return null;

        if (!symbols.Has(symbol, tokens.Source, out var definitions))
            return null;

        List<Location> locations = new(definitions.Count);
        var length = symbol.Length;
        var converter = tokens.MakeConverter();
        foreach (var (definition, _) in definitions)
        {
            var start = converter.LocalToProtocol(definition.Start);
            var end = start with { Character = start.Character + length };
            locations.Add(new Location(uri, new Range(start, end)));
        }
        await Logger.DebugAsync("textDocument/definition ->");
        return new DefinitionResponse(locations);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities _)
        => serverCapabilities.DefinitionProvider = true;
}