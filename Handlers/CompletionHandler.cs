using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Logging;
using NMLServer.Model.Grammar;

namespace NMLServer.Handlers;

internal sealed class CompletionHandler(SourceStorage storage) : CompletionHandlerBase
{
    protected override async Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token)
    {
        await Logger.DebugAsync("textDocument/completion <-");
        var doc = storage[request.TextDocument.Uri];
        List<CompletionItem> result = [];
        doc.ProvideCompletions(result);
        result.AddRange(builtins);
        await Logger.DebugAsync("textDocument/completion ->");
        return new CompletionResponse(result);
    }

    // TODO
    protected override async Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
    {
        await Logger.DebugAsync("completionItem/resolve <- / ->");
        return item;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.CompletionProvider = new CompletionOptions();
    }

    private static readonly List<CompletionItem> builtins = [];

    static CompletionHandler()
    {
        foreach (var kw in NML.Keywords.Dictionary.Keys)
            builtins.Add(new() { Label = kw, Kind = CompletionItemKind.Keyword });
        foreach (var unit in NML.UnitLiterals.Dictionary.Keys)
            builtins.Add(new() { Label = unit, Kind = CompletionItemKind.Keyword });
        foreach (var (label, kind) in NML.DefinedSymbols.Dictionary)
        {
            var cik = NML.GetCompletionItemKind(kind);
            if (cik != 0)
                builtins.Add(new CompletionItem { Label = label, Kind = cik });
        }
    }
}