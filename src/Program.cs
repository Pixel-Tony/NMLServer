using Microsoft.Extensions.DependencyInjection;
using NMLServer.Analysis;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;

namespace NMLServer;

internal class Program
{
    public static readonly TextDocumentSelector NMLSelector = new(
        new TextDocumentFilter { Language = "nml", Scheme = "file" }
    );

    public static ILanguageServer Server = null!;

    public static async Task Main()
    {
        SourceStorage storage = new();

        Server = await LanguageServer.From(
            new LanguageServerOptions()
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithServices(services => services.AddSingleton(storage))
                .AddHandler<TextDocumentSyncHandler>()
                // .AddHandler<DocumentDiagnosticHandler>()
                // .AddHandler<SemanticTokensHandler>()
                // .AddHandler<DefinitionHandler>()
        ).ConfigureAwait(false);

        // storage.ShouldPublishDiagnostics += Server.PublishDiagnostics;

        await Server.WaitForExit.ConfigureAwait(false);
    }
}