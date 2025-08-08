using NMLServer.Model.Diagnostics;
using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;

namespace NMLServer.Model.Statements.Procedures;

internal sealed class Procedure : BaseStatement
{
    private readonly KeywordToken _keyword;
    private readonly BaseExpression? _parameters;
    private readonly SemicolonToken? _semicolon;

    public override int Start => _keyword.Start;
    public override int End => _semicolon?.End ?? _parameters?.End ?? _keyword.End;

    public Procedure(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.Increment();
        _parameters = BaseExpression.TryParse(ref state);
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
                context.Add(expectParams, offset: _keyword.End);
            else
                context.Add(expectParams, item: _parameters);
            return;
        }
        if (opParen is null)
            context.Add(expectOpParen, inner?.Start ?? _keyword.End);

        if (clParen is null)
            context.Add(expectClParen, End);

        if (_semicolon is null)
            context.Add("Expected ;", End);
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