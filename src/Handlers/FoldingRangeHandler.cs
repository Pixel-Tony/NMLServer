using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model.Statement;

namespace NMLServer.Handlers;

class FoldingRangeHandler(SourceStorage storage) : FoldingRangeHandlerBase
{
    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.FoldingRangeProvider = true;
    }

    protected override async Task<FoldingRangeResponse> Handle(FoldingRangeParams request, CancellationToken token)
    {
        await Program.DebugAsync("textDocument/foldingRange <-");
        var doc = storage[request.TextDocument.Uri];
        var statements = doc.Statements;

        List<FoldingRange> ranges = [];
        if (statements.Count == 0)
            goto label_End;

        Stack<IFoldingRangeProvider> children = [];
        var converter = doc.Tokens.MakeConverter();

        foreach (var stmt in statements)
            if (stmt is IFoldingRangeProvider provider)
                children.Push(provider);

        while (children.TryPop(out var child))
            child.ProvideFoldingRanges(in children, in ranges, ref converter);

        label_End:
        await Program.DebugAsync("textDocument/foldingRange ->");
        return new FoldingRangeResponse(ranges);
    }
}
