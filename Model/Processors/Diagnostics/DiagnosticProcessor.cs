using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors.Diagnostics;

internal sealed class DiagnosticProcessor : IIncrementalNodeProcessor
{
    public List<Diagnostic> Content { get; private set; } = [];

    public void ProcessChangedSyntax(ref TreeTraverser traverser, BaseStatement? end, ref readonly IncrementContext context)
    {
        Content.Clear();
        ref readonly var ast = ref context.SyntaxTree;
        DiagnosticContext diagnostics = new();
        for (TreeTraverser trv = new(in ast); trv.Current is { } node; trv.Increment())
            node.ProvideDiagnostics(diagnostics);

        foreach (var token in ast.UnexpectedTokens)
            diagnostics.Add(ErrorStrings.Err_UnexpectedToken, token);

        Content = diagnostics.BuildDiagnostics(in ast.Tokens);
    }
}
