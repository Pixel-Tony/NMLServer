using Microsoft.Extensions.DependencyInjection;
using NMLServer.Analysis;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Server;

namespace NMLServer;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var storage = new SourceStorage();

        var server = await LanguageServer.From(
            options => options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithServices(services => services.AddSingleton(storage))
                // .AddHandler<SemanticTokensHandler>()
        ).ConfigureAwait(false);

        storage.ShouldPublishDiagnostics += server.PublishDiagnostics;

        await server.WaitForExit.ConfigureAwait(false);
    }
}