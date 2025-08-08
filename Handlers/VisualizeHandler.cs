using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using DotNetGraph.Compilation;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Model;
using System.Diagnostics;
using System.Text;
using EmmyLua.LanguageServer.Framework.Server;
using EmmyLua.LanguageServer.Framework;
using System.Text.Json;
using NMLServer.Extensions;
using System.Text.Json.Serialization;

namespace NMLServer.Handlers;


internal partial class VisualizeHandler(SourceStorage storage) : IJsonHandler
{
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(List<string>))]
    private partial class SerializerContext : JsonSerializerContext;

    public void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    { }

    public void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    { }

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("nml/debug/drawAST", async (m, _) =>
        {
            await Program.DebugAsync("nml/debug/drawAST <-");
            if (m.Params?.Deserialize(SerializerContext.Default.ListString) is not [{ } uri])
                throw new ArgumentException("Incorrect argument passed.");
            var doc = storage[uri];
            await Program.DebugAsync("nml/debug/drawAST ->");
            return JsonSerializer.SerializeToDocument(await VisualizeAsync(doc), SerializerContext.Default.String);
        });
    }

    private static async Task<string> VisualizeAsync(Document doc)
    {
        var graph = new DotGraph().WithIdentifier("MyGraph")
            .WithAttribute("bgcolor", "transparent")
            .WithAttribute("dpi", "400");
        var root = new DotNode().WithIdentifier("Root")
            .WithAttribute("fontname", "Consolas");
        graph.Add(root);

        var src = doc.AST.Tokens.Source;
        foreach (var child in doc.AST.Nodes)
        {
            if (graph.Elements.Count >= 400)
            {
                VizExtensions.MakeNode(graph, root, "...");
                break;
            }
            child.Visualize(graph, root, src);
        }
        await using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        await graph.CompileAsync(context).ConfigureAwait(false);
        var dotFilePath = Path.Join(Path.GetTempPath(), "nmlserver-graph.dot");
        {
            using var dotFile = File.Create(dotFilePath);
            var content = Encoding.UTF8.GetBytes(writer.GetStringBuilder().ToString());
            await dotFile.WriteAsync(content).ConfigureAwait(false);
        }
        var pngFilePath = Path.Join(Path.GetTempPath(), "nmlserver-graph.png");
        File.Create(pngFilePath).Close();
        await Process.Start("dot", ["-T", "png", "-o", pngFilePath, dotFilePath]).WaitForExitAsync().ConfigureAwait(false);
        return pngFilePath;
    }
}