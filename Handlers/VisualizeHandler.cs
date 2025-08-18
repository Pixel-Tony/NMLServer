using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using EmmyLua.LanguageServer.Framework;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Server;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using NMLServer.Extensions;
using NMLServer.Logging;
using NMLServer.Model.Syntax;
using NMLServer.Model.Statements.Blocks;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
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
            await Logger.DebugAsync("nml/debug/drawAST <-");
            if (m.Params?.Deserialize(SerializerContext.Default.ListString) is not [{ } uri])
                throw new ArgumentException("Incorrect argument passed.");
            var doc = storage[uri];
            await Logger.DebugAsync("nml/debug/drawAST ->");
            return JsonSerializer.SerializeToDocument(
                await VisualizeAsync(doc.AST),
                SerializerContext.Default.String);
        });
    }

    private static async Task<string> VisualizeAsync(AbstractSyntaxTree tree)
    {
        var graph = new DotGraph()
            .WithIdentifier("MyGraph")
            .WithAttribute("bgcolor", "transparent")
            .WithAttribute("dpi", "400");
        var root = new DotNode().WithIdentifier("Root")
            .WithAttribute("fontname", "Consolas");
        graph.Add(root);

        Stack<DotNode> parents = [];
        var parent = root;
        var source = tree.Tokens.Source;
        for (TreeTraverser trv = new(in tree); trv.Current is { } node; trv.Increment())
        {
            if (graph.Elements.Count >= 500)
            {
                VizExtensions.MakeNode(graph, root, "...");
                break;
            }
            var n = node.Visualize(graph, parent, source);
            if (trv.IsOnLastChild)
                parent = parents.Count > 0 ? parents.Pop() : root;
            if (node is BaseParentStatement)
            {
                parents.Push(parent);
                parent = n;
            }
        }

        var dotFilePath = Path.Join(Path.GetTempPath(), "nmlserver-graph.dot");
        {
            await using var writer = new StringWriter();
            var context = new CompilationContext(writer, new CompilationOptions());
            await graph.CompileAsync(context).ConfigureAwait(false);
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