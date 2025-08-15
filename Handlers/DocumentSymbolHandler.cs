using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Logging;

namespace NMLServer.Handlers;

internal class DocumentSymbolHandler(SourceStorage storage) : DocumentSymbolHandlerBase
{
    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        => serverCapabilities.DocumentSymbolProvider = true;

    protected override async Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken token)
    {
        await Logger.DebugAsync("textDocument/documentSymbol <-/->");
        return new DocumentSymbolResponse(storage[request.TextDocument.Uri].DocumentSymbols.Content);
    }
}
