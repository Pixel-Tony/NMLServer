using System.Diagnostics;
using EmmyLua.LanguageServer.Framework.Server;
using NMLServer.Handlers;

namespace NMLServer;

internal static class Program
{
    public static void Debug(object? message) => Debug(message?.ToString());

    [Conditional("DEBUG")]
    public static void Debug(string? message)
    {
        Console.Error.WriteLine(message);
    }

    public static async Task DebugAsync(string? message)
    {
#if DEBUG
        await Console.Error.WriteLineAsync(message);
#endif
    }


    public static LanguageServer Server = null!;

    private static async Task Main()
    {
        SourceStorage storage = [];
        Server = LanguageServer.From(Console.OpenStandardInput(), Console.OpenStandardOutput());

        Server.OnInitialize((initParams, info) =>
        {
            info.Name = "NewGRF Meta Language";
            info.Version = "1.0.0";
            return Task.CompletedTask;
        });
        Server.AddHandler(new TextDocumentSyncHandler(storage))
            // .AddHandler(new DefinitionHandler(storage))
            .AddHandler(new SemanticTokensHandler(storage))
            // .AddHandler(new DocumentSymbolHandler(storage))
            // .AddHandler(new FoldingRangeHandler(storage))
            // .AddHandler(new CompletionHandler(storage))
            // .AddHandler(new DiagnosticsHandler(storage))
            // .AddHandler(new VisualizeHandler(storage))
            ;

        await Server.Run().ConfigureAwait(false);
    }
}