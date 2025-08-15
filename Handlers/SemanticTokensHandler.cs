using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Logging;
using NMLServer.Model.Grammar;

namespace NMLServer.Handlers;

internal class SemanticTokensHandler(SourceStorage storage) : SemanticTokensHandlerBase
{
    protected override async Task<SemanticTokens?> Handle(SemanticTokensParams request, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/semanticTokens/full <-");
        SemanticTokensBuilder builder = new(_tokenTypes, _tokenModifiers);
        var document = storage[request.TextDocument.Uri];
        document.AST.Tokens.ProvideSemanticTokens(builder, document.Definitions.Symbols);
        SemanticTokens result = new() { Data = builder.Build() };
        await Logger.DebugAsync("textDocument/semanticTokens/full ->");
        return result;
    }

    protected override async Task<SemanticTokens?> Handle(SemanticTokensRangeParams request, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/semanticTokens/range <-");
        SemanticTokensBuilder builder = new(_tokenTypes, _tokenModifiers);
        var document = storage[request.TextDocument.Uri];
        document.AST.Tokens.ProvideSemanticTokens(builder, request.Range, document.Definitions.Symbols);
        SemanticTokens result = new() { Data = builder.Build() };
        await Logger.DebugAsync("textDocument/semanticTokens/range ->");
        return result;
    }

    protected override async Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams request, CancellationToken _)
    {
        await Logger.DebugAsync("textDocument/semanticTokens/full/delta <-");
        throw new NotSupportedException("Request <textDocument/semanticTokens/full/delta> is not supported.");
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
                TokenTypes = _tokenTypes,
                TokenModifiers = _tokenModifiers
            }
        };
    }

    private static readonly List<string> _tokenTypes =
    [
        SemanticTokenTypes.EnumMember, SemanticTokenTypes.Parameter, SemanticTokenTypes.Number, SemanticTokenTypes.Type,
        SemanticTokenTypes.String, SemanticTokenTypes.Keyword, SemanticTokenTypes.Variable, SemanticTokenTypes.Comment,
        SemanticTokenTypes.Operator, SemanticTokenTypes.Function, SemanticTokenTypes.Property, ExtraSemanticTokenTypes.Constant
    ];

    private static readonly List<string> _tokenModifiers = [];
}