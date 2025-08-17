using EmmyLua.LanguageServer.Framework.Server;
using NMLServer.Handlers;

namespace NMLServer;

internal static class Program
{
    private static async Task Main()
    {
        SourceStorage storage = [];
        var Server = LanguageServer.From(Console.OpenStandardInput(), Console.OpenStandardOutput());

        Server.OnInitialize((initParams, info) =>
        {
            info.Name = "NewGRF Meta Language";
            info.Version = "1.0.0";
            return Task.CompletedTask;
        });
        var clientProxy = Server.Client;
        Server.AddHandler(new TextDocumentSyncHandler(storage, clientProxy))
            .AddHandler(new DefinitionHandler(storage))
            .AddHandler(new SemanticTokensHandler(storage))
            .AddHandler(new FoldingRangeHandler(storage))
            .AddHandler(new CompletionHandler(storage))
            .AddHandler(new DiagnosticsHandler(storage))
            .AddHandler(new VisualizeHandler(storage))
            ;

        await Server.Run().ConfigureAwait(false);
    }
}