using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace NMLServer.Handlers;

internal class CompletionHandler(SourceStorage storage) : CompletionHandlerBase
{
    protected override Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token)
    {
        Program.Debug("textDocument/completion <-");
        var doc = storage[request.TextDocument.Uri];
        var completions = doc.ProvideCompletions(request.Position);
        Program.Debug("textDocument/completion ->");
        return Task.FromResult<CompletionResponse?>(new CompletionResponse(completions));
    }

    protected override Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
    {
        Program.Debug("completionItem/resolve <- / ->");
        return Task.FromResult(item); // TODO
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.CompletionProvider = new CompletionOptions();
    }
}