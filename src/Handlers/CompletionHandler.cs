using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace NMLServer.Handlers;

internal sealed class CompletionHandler(SourceStorage storage) : CompletionHandlerBase
{
    protected override async Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token)
    {
        await Program.DebugAsync("textDocument/completion <-");
        var doc = storage[request.TextDocument.Uri];
        var completions = doc.ProvideCompletions(request.Position);
        await Program.DebugAsync("textDocument/completion ->");
        return new CompletionResponse(result);
    }

    // TODO
    protected override async Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
    {
        await Program.DebugAsync("completionItem/resolve <- / ->");
        return item;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.CompletionProvider = new CompletionOptions();
    }
}