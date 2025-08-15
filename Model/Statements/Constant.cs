using NMLServer.Model.Expressions;
using NMLServer.Model.Tokens;
using NMLServer.Extensions;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Statements;

internal class Constant : BaseStatement
{
    private readonly KeywordToken _keyword;
    private readonly IdentifierToken? _lhs;
    private readonly AssignmentToken? _equalsSign;
    private readonly BaseExpression? _rightHandSide;
    private readonly SemicolonToken? _semicolon;

    public override int Start => _keyword.Start;

    public override int End
        => _semicolon?.End ?? _rightHandSide?.End ?? _equalsSign?.End ?? _lhs?.End ?? _keyword.End;

    public Constant(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        while (state.NextToken is { } token)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                case KeywordToken { IsDefiningStatement: true }:
                    return;

                case IdentifierToken { Kind: SymbolKind.None } id when _lhs is null:
                    _lhs = id;
                    break;

                case AssignmentToken equalsSign:
                    _equalsSign = equalsSign;
                    state.Increment();
                    _rightHandSide = BaseExpression.TryParse(ref state);
                    _semicolon = state.ExpectSemicolon();
                    return;

                case SemicolonToken semicolon:
                    _semicolon = semicolon;
                    state.Increment();
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }

    public override void AddDefinitions(DefinitionBag bag, StringView source)
    {
        if (_lhs is not null)
            bag.Add(_lhs, SymbolKind.Constant, source);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Constant");
        _keyword.Visualize(graph, n, ctx);
        _lhs.MaybeVisualize(graph, n, ctx);
        _equalsSign.MaybeVisualize(graph, n, ctx);
        _rightHandSide.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}