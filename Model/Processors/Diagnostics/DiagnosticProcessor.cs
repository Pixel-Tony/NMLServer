using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors.Diagnostics;

internal sealed class DiagnosticProcessor : IIncrementalNodeProcessor
{
    public List<Diagnostic> Content { get; private set; } = [];

    public void FinishIncrement(ref readonly AbstractSyntaxTree ast)
    {
        Content.Clear();
        DiagnosticContext context = new();
        for (TreeTraverser trv = new(ast); trv.Current is { } node; trv.Increment())
            node.ProvideDiagnostics(context);

        foreach (var token in ast.UnexpectedTokens)
            context.Add("Unexpected token", token);

        Content = context.BuildDiagnostics(in ast.Tokens);
    }

    public void Process(BaseStatement node, NodeProcessingContext context)
    {
        //
    }

    public void Trim()
    {
        //
    }
}
