using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model;
using NMLServer.Model.Statement;

namespace NMLServer.Handlers;

internal class DocumentSymbolHandler(SourceStorage storage) : DocumentSymbolHandlerBase
{
    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        => serverCapabilities.DocumentSymbolProvider = true;

    protected override async Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken token)
    {
        await Program.DebugAsync("textDocument/documentSymbol <-");
        var doc = storage[request.TextDocument.Uri];
        var statements = doc.Statements;
        var tokens = doc.Tokens;
        List<DocumentSymbol> symbols = [];
        if (statements.Count == 0)
            goto label_End;

        Stack<(InnerStatementNode node, List<DocumentSymbol> parentList)> parents = [];
        var converter = tokens.MakeConverter();

        foreach (var child in statements)
            TryAddSymbol(child, parents, symbols, ref tokens, ref converter);

        while (parents.TryPop(out var parent))
        {
            if (parent.node.Children is { } children)
                foreach (var child in children)
                    TryAddSymbol(child, parents, parent.parentList, ref tokens, ref converter);
        }

    label_End:
        await Program.DebugAsync("textDocument/documentSymbol ->");
        return new DocumentSymbolResponse(symbols);
    }

    private static void TryAddSymbol(
        StatementAST child,
        Stack<(InnerStatementNode node, List<DocumentSymbol> children)> parents,
        List<DocumentSymbol> symbols,
        ref readonly TokenStorage tokens,
        ref TokenStorage.PositionConverter converter)
    {
        List<DocumentSymbol> parentList = [];

        var symbol = (child as ISymbolSource)?.Symbol;
        if (child is InnerStatementNode parent)
            parents.Push((parent, symbol is null ? symbols : parentList));
        if (symbol is null)
            return;

        var start = converter.LocalToProtocol(child.Start);
        var symPos = converter.LocalToProtocol(symbol.Start);
        var end = converter.Copy().LocalToProtocol(child.End);
        var range = new Range(start, end);
        var symRange = new Range(symPos, symPos with { Character = symPos.Character + symbol.Length });
        symbols.Add(new DocumentSymbol()
        {
            Kind = Grammar.GetDocumentSymbolKind(symbol.Kind),
            Children = parentList,
            Range = range,
            SelectionRange = symRange,
            Name = tokens.GetSymbolContext(symbol).ToString()
        });
    }
}
