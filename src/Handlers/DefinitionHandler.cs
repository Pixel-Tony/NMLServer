using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using static NMLServer.RangeExtensions;

namespace NMLServer.Analysis;

internal class DefinitionHandler(SourceStorage storage) : DefinitionHandlerBase
{
    public override Task<LocationOrLocationLinks?> Handle(DefinitionParams request, CancellationToken _)
        => Task.FromResult(GetDefinitions(request));

    protected override DefinitionRegistrationOptions CreateRegistrationOptions(DefinitionCapability capability,
        ClientCapabilities clientCapabilities) => new() { DocumentSelector = Program.NMLSelector };

    private LocationOrLocationLinks? GetDefinitions(DefinitionParams request)
    {
        var document = storage.GetDocument(request);
        if (document.GetToken(request.Position) is not IdentifierToken symbol)
        {
            return null;
        }

        var definitions = document.GetDefinitions(symbol);
        if (definitions is null)
        {
            return null;
        }

        List<LocationOrLocationLink> locations = new(definitions.Count);
        var length = symbol.length;
        var uri = document.Uri;
        var converter = document.GetConverter();
        for (var i = 0; i < locations.Count; i++)
        {
            var definitionStart = definitions[i].start;
            var (line, @char) = converter.LocalToProtocol(definitionStart);
            locations[i] = new Location { Uri = uri, Range = RangeFromRaw(line, @char, length) };
        }

        return locations;
    }
}