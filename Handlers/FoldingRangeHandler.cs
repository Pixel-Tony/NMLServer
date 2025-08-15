using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Logging;

namespace NMLServer.Handlers;

internal class FoldingRangeHandler(SourceStorage storage) : FoldingRangeHandlerBase
{
    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.FoldingRangeProvider = true;
    }

    protected override async Task<FoldingRangeResponse> Handle(FoldingRangeParams request, CancellationToken token)
    {
        await Logger.DebugAsync("textDocument/foldingRange <- / ->");
        var doc = storage[request.TextDocument.Uri];
        var ranges = doc.FoldingRanges.Content;
        return new FoldingRangeResponse(ranges);
    }
}
