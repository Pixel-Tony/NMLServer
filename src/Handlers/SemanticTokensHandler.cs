using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model;

namespace NMLServer.Handlers;

internal class SemanticTokensHandler(SourceStorage storage) : SemanticTokensHandlerBase
{
    protected override async Task<SemanticTokens?> Handle(SemanticTokensParams request, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/semanticTokens/full <-");
        SemanticTokensBuilder builder = new(Grammar.TokenTypes, Grammar.TokenModifiers);
        var document = storage[request.TextDocument.Uri];
        document.Tokens.ProvideSemanticTokens(in builder, document);
        SemanticTokens result = new() { Data = builder.Build() };
        await Program.DebugAsync("textDocument/semanticTokens/full ->");
        return result;
    }

    protected override async Task<SemanticTokens?> Handle(SemanticTokensRangeParams request, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/semanticTokens/range <-");
        SemanticTokensBuilder builder = new(Grammar.TokenTypes, Grammar.TokenModifiers);
        var document = storage[request.TextDocument.Uri];
        document.Tokens.ProvideSemanticTokens(in builder, request.Range, document);
        SemanticTokens result = new() { Data = builder.Build() };
        await Program.DebugAsync("textDocument/semanticTokens/range ->");
        return result;
    }

    protected override async Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams request, CancellationToken _)
    {
        await Program.DebugAsync("textDocument/semanticTokens/full/delta <-");
        throw new NotImplementedException("The server does not support incremental SemanticTokens requests.");
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities _)
    {
        serverCapabilities.SemanticTokensProvider = new SemanticTokensOptions
        {
            Full = new SemanticTokensCapabilitiesFull
            {
                Delta = false
            },
            Range = true,
            Legend = new SemanticTokensLegend
            {
                TokenTypes = Grammar.TokenTypes,
                TokenModifiers = Grammar.TokenModifiers
            }
        };
    }
}