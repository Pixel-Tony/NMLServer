using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model;
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

        Stack<InnerStatementNode> parents = [];
        var converter = doc.Tokens.MakeConverter();

        foreach (var child in statements)
            TryAddRange(child, parents, ranges, ref converter);

        while (parents.TryPop(out var parent))
        {
            if (parent.Children is { } children)
                foreach (var child in children)
                    TryAddRange(child, parents, ranges, ref converter);
        }
    label_End:
        await Program.DebugAsync("textDocument/foldingRange ->");
        return new FoldingRangeResponse(ranges);
    }


    private static void TryAddRange(StatementAST child, Stack<InnerStatementNode> parents, List<FoldingRange> ranges,
        ref TokenStorage.PositionConverter converter)
    {
        if (child is InnerStatementNode parent)
            parents.Push(parent);

        if (child is not BlockStatement { OpeningBracket.Start: { } start, ClosingBracket.End: { } end })
            return;

        var startPos = converter.LocalToProtocol(start);
        var endPos = converter.LocalToProtocol(end);
        ranges.Add(new FoldingRange()
        {
            Kind = FoldingRangeKind.Region,
            StartLine = (uint)startPos.Line,
            StartCharacter = (uint)startPos.Character,
            EndLine = (uint)endPos.Line,
            EndCharacter = (uint)endPos.Character,
        });
    }
}
