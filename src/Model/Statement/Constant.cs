#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions.DotNetGraph;
#endif
using NMLServer.Model.Expression;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class Constant : StatementAST, ISymbolSource
{
    private readonly KeywordToken _keyword;
    private readonly IdentifierToken? _leftHandSide;
    private readonly AssignmentToken? _equalsSign;
    private readonly ExpressionAST? _rightHandSide;
    private readonly SemicolonToken? _semicolon;

    public override int Start => _keyword.Start;

    public override int End
        => _semicolon?.End ?? _rightHandSide?.End ?? _equalsSign?.End ?? _leftHandSide?.End ?? _keyword.End;

    public IdentifierToken? Symbol { get; }

    public Constant(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        if (state.NextToken is not IdentifierToken identifier)
            return;
        _leftHandSide = identifier;
        if (identifier.Kind is SymbolKind.Undefined)
        {
            identifier.Kind = SymbolKind.Constant;
            Symbol = identifier;
        }
        while (state.NextToken is { } token)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    return;
                case AssignmentToken equalsSign:
                    _equalsSign = equalsSign;
                    state.IncrementSkippingComments();
                    _rightHandSide = ExpressionAST.TryParse(ref state);
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

#if TREE_VISUALIZER_ENABLED

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Constant");
        _keyword.Visualize(graph, n, ctx);
        _leftHandSide.MaybeVisualize(graph, n, ctx);
        _equalsSign.MaybeVisualize(graph, n, ctx);
        _rightHandSide.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}