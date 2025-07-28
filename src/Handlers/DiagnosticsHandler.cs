using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentDiagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Statement;

namespace NMLServer.Handlers;

internal class DiagnosticsHandler(SourceStorage storage) : DocumentDiagnosticHandlerBase
{
    protected override async Task<DocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/diagnostic <-");
        var doc = storage[request.TextDocument.Uri];
        var diagnostics = ProvideDiagnostics(doc);
        RelatedFullDocumentDiagnosticReport result = new() { Diagnostics = diagnostics };
        await Program.DebugAsync("textDocument/diagnostic ->");
        return result;
    }

    public static List<Diagnostic> ProvideDiagnostics(Document doc)
    {
        var statements = doc.Statements;
        var tokens = doc.Tokens;
        var converter = tokens.MakeConverter();
        DiagnosticContext context = new(ref converter);
        foreach (var unexpectedToken in doc.UnexpectedTokens)
            context.Add("Unexpected token", unexpectedToken);
        if (statements.Count == 0)
            return [];
        Stack<InnerStatementNode> parents = [];
        foreach (var child in statements)
            SupplyDiagnostics(child, parents, ref context, doc);
        while (parents.TryPop(out var node))
            if (node.Children is { } children)
                foreach (var child in children)
                    SupplyDiagnostics(child, parents, ref context, doc);
        return context.Diagnostics;
    }

    private static void SupplyDiagnostics(StatementAST node, Stack<InnerStatementNode> parents, ref DiagnosticContext context, IDefinitionsBag bag)
    {
        (node as IDiagnosticProvider)?.VerifySyntax(ref context);
        (node as IContextProvider)?.VerifyContext(ref context, bag);
        if (node is InnerStatementNode parent)
            parents.Push(parent);
    }

    public override void RegisterCapability(ServerCapabilities server, ClientCapabilities _)
    {
        server.DiagnosticProvider = new DiagnosticOptions();
    }
}