using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;
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

    public override int start => _keyword.start;

    public override int end
        => _semicolon?.end ?? _rightHandSide?.end ?? _equalsSign?.end ?? _leftHandSide?.end ?? _keyword.end;

    public IdentifierToken? symbol { get; }

    public Constant(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        if (state.nextToken is not IdentifierToken identifier)
            return;
        _leftHandSide = identifier;
        if (identifier.Kind is SymbolKind.Undefined)
        {
            identifier.Kind = SymbolKind.Constant;
            symbol = identifier;
        }
        while (state.nextToken is { } token)
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    return;
                case AssignmentToken equalsSign:
                    _equalsSign = equalsSign;
                    state.IncrementSkippingComments();
                    goto label_AfterSign;
                case SemicolonToken semicolon:
                    _semicolon = semicolon;
                    state.Increment();
                    return;
                default:
                    state.AddUnexpected(token);
                    break;
            }
        label_AfterSign:
        _rightHandSide = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }

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
}