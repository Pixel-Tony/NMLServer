using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed class Assignment : StatementAST, ISymbolSource, IDiagnosticProvider
{
    private readonly ExpressionAST _leftHandSide;
    private readonly AssignmentToken? _equalsSign;
    private readonly ExpressionAST? _rightHandSide;
    private readonly SemicolonToken? _semicolon;

    public IdentifierToken? symbol
        => _equalsSign is not null && _leftHandSide is Identifier { kind: SymbolKind.Undefined } id
            ? id.token
            : null;

    public override int start => _leftHandSide.start;
    public override int end => _semicolon?.end ?? _rightHandSide?.end ?? _equalsSign?.end ?? _leftHandSide.end;

    public Assignment(ref ParsingState state)
    {
        _leftHandSide = ExpressionAST.TryParse(ref state)!;
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
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
        }
        label_AfterSign:
        _rightHandSide = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
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
                    context.Add(Errors.MissingClosingBracket, arguments.end);
                else if (closing.Bracket is not ']')
                    context.Add(Errors.ExpectedSquareBracket, closing.start);
                break;

            default:
                context.Add(Errors.InvalidTarget, _leftHandSide);
                break;
        }
        if (_rightHandSide is null)
        {
            var pos = _equalsSign.end;
            context.Add(Errors.MissingAssignedValue, pos);
            if (_semicolon is null)
                context.Add(Errors.MissingSemicolon, pos);
            return;
        }

        _rightHandSide.VerifySyntax(in context);
        if (_semicolon is null)
            context.Add(Errors.MissingSemicolon, _rightHandSide.end);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Assignment");
        _leftHandSide.MaybeVisualize(graph, n, ctx);
        _equalsSign.MaybeVisualize(graph, n, ctx);
        _rightHandSide.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}