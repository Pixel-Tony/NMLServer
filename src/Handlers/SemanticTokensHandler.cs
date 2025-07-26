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
    protected override Task<SemanticTokens?> Handle(SemanticTokensParams request, CancellationToken _)
    {
        Program.Debug("textDocument/semanticTokens/full <-");
        SemanticTokensBuilder builder = new(Grammar.TokenTypes, Grammar.TokenModifiers);
        var document = storage[request.TextDocument.Uri];
        document.ProvideSemanticTokens(in builder);
        SemanticTokens result = new() { Data = builder.Build() };
        Program.Debug("textDocument/semanticTokens/full ->");
        return Task.FromResult<SemanticTokens?>(result);
    }

    protected override Task<SemanticTokens?> Handle(SemanticTokensRangeParams request, CancellationToken _)
    {
        Program.Debug("textDocument/semanticTokens/range <-");
        SemanticTokensBuilder builder = new(Grammar.TokenTypes, Grammar.TokenModifiers);
        var document = storage[request.TextDocument.Uri];
        document.ProvideSemanticTokens(in builder, request.Range);
        SemanticTokens result = new() { Data = builder.Build() };
        Program.Debug("textDocument/semanticTokens/range ->");
        return Task.FromResult<SemanticTokens?>(result);
    }

    protected override Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams request, CancellationToken _)
    {
        Program.Debug("textDocument/semanticTokens/full/delta <-");
        return Task.FromException<SemanticTokensDeltaResponse?>(
            new NotImplementedException("The server does not support incremental SemanticTokens requests.")
        );
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