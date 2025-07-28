using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model;

namespace NMLServer.Handlers;

internal sealed class CompletionHandler(SourceStorage storage) : CompletionHandlerBase
{
    protected override async Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token)
    {
        await Program.DebugAsync("textDocument/completion <-");
        var doc = storage[request.TextDocument.Uri];
        List<CompletionItem> result = [];
        doc.ProvideCompletions(ref result);
        result.AddRange(builtins);
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

    private static readonly List<CompletionItem> builtins = [];

    static CompletionHandler()
    {
        foreach (var (kw, type) in Grammar.Keywords.Dictionary)
        {
            builtins.Add(new()
            {
                Label = kw,
                Kind = CompletionItemKind.Keyword,
                InsertTextFormat = InsertTextFormat.Snippet,
                InsertText = (type.kind & KeywordKind.BlockDefining) != 0
                ? kw + " ${1:($2) }{\n\t$0\n}"
                : kw
            });
        }
        foreach (var unit in Grammar.UnitLiterals.Dictionary.Keys)
            builtins.Add(new() { Label = unit, Kind = CompletionItemKind.Keyword });
        foreach (var (label, kind) in Grammar.DefinedSymbols.Dictionary)
        {
            var cik = Grammar.GetCompletionItemKind(kind);
            if (cik != 0)
                builtins.Add(new CompletionItem { Label = label, Kind = cik });
        }
    }
}