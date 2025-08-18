using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;

namespace NMLServer.Model.Statements.Procedures;

internal sealed class Procedure : BaseStatement
{
    private readonly KeywordToken _keyword;
    private readonly BaseExpression? _arguments;
    private readonly SemicolonToken? _semicolon;

    public override int Start => _keyword.Start;
    public override int End => _semicolon?.End ?? _arguments?.End ?? _keyword.End;

    public Procedure(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.Increment();
        _arguments = BaseExpression.TryParse(ref state);
        (_arguments as ParentedExpression)?.ConvertToRightAssociative();
        _semicolon = state.ExpectSemicolon();
    }

    public override void ProvideDiagnostics(DiagnosticContext context)
    {
        ProcessArgumentList(context, _keyword, _arguments);
        if (_semicolon is null)
            context.Add(ErrorStrings.Err_MissingSemicolon, End);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Call");
        _keyword.Visualize(graph, n, ctx);
        _arguments.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}