using Microsoft.Extensions.DependencyInjection;
using NMLServer.Analysis;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using OmniSharp.Extensions.LanguageServer.Server;

namespace NMLServer;

internal class Program
{
    public static readonly TextDocumentSelector NMLSelector = new(
        new TextDocumentFilter { Language = "nml", Scheme = "file" }
    );

    private static ILanguageServer? _server;

    public static void LogInfo(string message) => _server?.LogInfo(message);

    public static async Task Main()
    {
        var storage = new SourceStorage();

        _server = await LanguageServer.From(
            options => options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithServices(services => services.AddSingleton(storage))
                .AddHandler<TextDocumentSyncHandler>()
                .AddHandler<DocumentDiagnosticHandler>()
                .AddHandler<SemanticTokensHandler>()
        ).ConfigureAwait(false);

        storage.ShouldPublishDiagnostics += _server.PublishDiagnostics;

        await _server.WaitForExit.ConfigureAwait(false);
    }
}