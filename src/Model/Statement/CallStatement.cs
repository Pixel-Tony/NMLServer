using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed class CallStatement : StatementAST, IDiagnosticProvider
{
    private readonly KeywordToken _keyword;
    private readonly ExpressionAST? _parameters;
    private readonly SemicolonToken? _semicolon;

    public override int start => _keyword.start;
    public override int end => _semicolon?.end ?? _parameters?.end ?? _keyword.end;

    public CallStatement(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.IncrementSkippingComments();
        _parameters = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }

    public void VerifySyntax(ref readonly DiagnosticContext context)
    {
        const string expectParams = "Expected a parameter sequence enclosed in parens";
        const string expectOpParen = "Expected \"(\"";
        const string expectClParen = "Expected \")\"";

        if (_parameters is not ParentedExpression
            {
                OpeningBracket: var opParen,
                Expression: var inner,
                ClosingBracket: var clParen
            })
        {
            if (_parameters is null)
                context.Add(expectParams, _keyword.end);
            else
                context.Add(expectParams, _parameters);
            return;
        }
        if (opParen is null)
            context.Add(expectOpParen, inner?.start ?? _keyword.end);

        if (clParen is null)
            context.Add(expectClParen, end);

        if (_semicolon is null)
            context.Add("Expected ;", end);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Call");
        _keyword.Visualize(graph, n, ctx);
        _parameters.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}