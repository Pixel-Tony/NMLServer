using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentDiagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;

namespace NMLServer.Handlers;

internal class DiagnosticsHandler(SourceStorage storage) : DocumentDiagnosticHandlerBase
{
    protected override async Task<DocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/diagnostic <-");
        var doc = storage[request.TextDocument.Uri];
        // var diagnostics = ProvideDiagnostics(doc);
        List<Diagnostic> diagnostics = [];
        RelatedFullDocumentDiagnosticReport result = new() { Diagnostics = diagnostics };
        await Program.DebugAsync("textDocument/diagnostic ->");
        return result;
    }

    // public static List<Diagnostic> ProvideDiagnostics(Document doc)
    // {
    //     var statements = doc.AST.Nodes;
    //     var tokens = doc.AST.Tokens;
    //     var converter = tokens.MakeConverter();
    //     DiagnosticContext context = new(ref converter);
    //     foreach (var unexpectedToken in doc.AST.UnexpectedTokens)
    //         context.Add("Unexpected token", unexpectedToken);
    //     if (statements.Count == 0)
    //         return [];

    //     ref var defs = ref doc.Definitions;
    //     Stack<BaseParentStatement> parents = [];
    //     foreach (var child in statements)
    //         SupplyDiagnostics(child, parents, ref context, ref defs);
    //     while (parents.TryPop(out var node))
    //         if (node.Children is { } children)
    //             foreach (var child in children)
    //                 SupplyDiagnostics(child, parents, ref context, ref defs);
    //     return context.Diagnostics;
    // }

    public override void RegisterCapability(ServerCapabilities server, ClientCapabilities _)
    {
        server.DiagnosticProvider = new DiagnosticOptions();
    }
}