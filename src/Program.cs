using EmmyLua.LanguageServer.Framework.Server;
using NMLServer.Handlers;

namespace NMLServer;

internal static class Program
{
    private const string Name = "NewGRF Meta Language";
    private const string Version = "1.0.0";

    public static LanguageServer Server = null!;

    private static async Task Main(string[] args)
    {
        SourceStorage storage = new();
        Server = LanguageServer.From(Console.OpenStandardInput(), Console.OpenStandardOutput());

        Server.OnInitialize((initParams, info) =>
        {
            info.Name = Name;
            info.Version = Version;
            return Task.CompletedTask;
        });
        Server.AddHandler(new TextDocumentSyncHandler(storage))
            .AddHandler(new DefinitionHandler(storage))
            .AddHandler(new SemanticTokensHandler(storage))
            .AddHandler(new CompletionHandler(storage))
            .AddHandler(new DiagnosticsHandler(storage));

        await Server.Run().ConfigureAwait(false);
    }
}