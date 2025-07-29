using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Statement;

internal sealed class Assignment : StatementAST, ISymbolSource, IDiagnosticProvider
{
    private readonly ExpressionAST _leftHandSide;
    private readonly AssignmentToken? _equalsSign;
    private readonly ExpressionAST? _rightHandSide;
    private readonly SemicolonToken? _semicolon;

    public IdentifierToken? Symbol { get; }

    public override int Start => _leftHandSide.Start;
    public override int End => _semicolon?.End ?? _rightHandSide?.End ?? _equalsSign?.End ?? _leftHandSide.End;

    public Assignment(ref ParsingState state)
    {
        _leftHandSide = ExpressionAST.TryParse(ref state)!;
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining }:
                    return;
                case AssignmentToken equalsSign:
                    _equalsSign = equalsSign;
                    if (_leftHandSide is Identifier { Kind: SymbolKind.Undefined, Token: var id })
                    {
                        id.Kind = SymbolKind.Variable;
                        Symbol = id;
                    }
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
                    state.Increment();
                    break;
            }
        }
    }

    public void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (_equalsSign is null)
        {
            context.Add(Errors.UnexpectedTopLevelExpr, _leftHandSide);
            return;
        }
        switch (_leftHandSide)
        {
            // TODO behavior for previously defined parameters
            case Identifier:
                break;

            case FunctionCall
            {
                Function: KeywordToken { Type: KeywordType.Param },
                Arguments: { OpeningBracket: var opening, ClosingBracket: var closing } arguments
            }:
                if (opening!.Bracket is not '[')
                    context.Add(Errors.ExpectedSquareBracket, opening);
                if (closing is null)
                    context.Add(Errors.MissingClosingBracket, arguments.End);
                else if (closing.Bracket is not ']')
                    context.Add(Errors.ExpectedSquareBracket, closing.Start);
                break;

            default:
                context.Add(Errors.InvalidTarget, _leftHandSide);
                break;
        }
        if (_rightHandSide is null)
        {
            var pos = _equalsSign.End;
            context.Add(Errors.MissingAssignedValue, pos);
            if (_semicolon is null)
                context.Add(Errors.MissingSemicolon, pos);
            return;
        }

        _rightHandSide.VerifySyntax(in context);
        if (_semicolon is null)
            context.Add(Errors.MissingSemicolon, _rightHandSide.End);
    }

#if TREE_VISUALIZER_ENABLED
    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Assignment");
        _leftHandSide.Visualize(graph, n, ctx);
        _equalsSign.MaybeVisualize(graph, n, ctx);
        _rightHandSide.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
#endif
}